using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SaurianSource.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace SaurianSource
{
    public class DinoController : Controller   
    {

        private MyContext _context;

        // this is to keep random people from being able to register
        // when the site is live on the Internet
        private string registerPassword;
     
        public DinoController(MyContext context, IConfiguration configuration)
        {
            _context = context;
            registerPassword = configuration["SecretPassword:Password"];
        }

        [HttpGet("")]
        public IActionResult Index() 
        {
            return View("index");
        }

        [HttpGet("/register")]
        public IActionResult Register()
        {
            return View("register");
        }

        [HttpPost("/register")]
        public IActionResult Register(User user, IFormFile AvatarFile, string registerPW)
        {
            if (registerPW.ToLowerInvariant() != registerPassword)
            {
                // not allowed to register
                ViewBag.Error = "Invalid registration code!";
                return View("Register");
            }

            if (ModelState.IsValid)
            {
                // enforce unique user
                User checkUser = _context.Users
                    .Where(u => user.Email == u.Email)
                    .FirstOrDefault();

                if (checkUser != null)
                {
                    ViewBag.Error = "User with that email already exists!";
                    return View("Register");
                }

                // hash password
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);

                _context.Add(user);

                // deal with uploaded file
                if (AvatarFile != null && AvatarFile.Length > 0)
                {
                    string extension = Path.GetExtension(AvatarFile.FileName);
                    // set the user's name as the filename
                    string fileName = user.Name + extension;
                    // save it to the avatars folder
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\avatars", fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        AvatarFile.CopyTo(fileStream);
                    }

                    user.AvatarUrl = @"~/avatars/" + fileName;
                }
                else 
                {
                    user.AvatarUrl = @"~/avatars/default.png";
                }

                _context.SaveChanges();

                HttpContext.Session.SetString("logged_in_user", user.Email);

                return RedirectToAction("Threads");
            }
            else 
            {
                return View("register");
            }
        }

        [HttpPost("/login")]
        public IActionResult Login(string Email, string Password)
        {
            if (string.IsNullOrEmpty(Email))
            {             
                ViewBag.Error = "You need to enter an email!";
                return View("Index");
            }

            if (string.IsNullOrEmpty(Password))
            {
                ViewBag.Error = "You need to enter a password!";
                return View("Index");
            }

            User attemptingUser = _context.Users
                .Where(u => u.Email == Email)
                .SingleOrDefault();

            if (attemptingUser == null) 
            {
                // user does not exist
                ViewBag.Error = "No user with that email exists!";
                return View("Index");
            }

            PasswordHasher<User> hasher = new PasswordHasher<User>();
            PasswordVerificationResult result = hasher.VerifyHashedPassword(attemptingUser, attemptingUser.Password, Password);

            if (result == 0) 
            {
                // invalid password
                ViewBag.Error = "INVALID PASSWORD!";
                return View("Index");
            }

            HttpContext.Session.SetString("logged_in_user", attemptingUser.Email);

            return RedirectToAction("Threads");
        }

        [HttpPost("/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("logged_in_user");
            return RedirectToAction("Index");
        }

        [HttpGet("/threads")]
        public IActionResult Threads()
        {
            // make sure user is logged in
            string userEmail = HttpContext.Session.GetString("logged_in_user");

            if (userEmail == null)
            {
                return RedirectToAction("Index");
            }

            User loggedInUser = _context.Users
                .Where(u => u.Email == userEmail)
                .SingleOrDefault();

            List<Thread> threads = _context.Threads
                .Include(t => t.User)
                .Include(t => t.Messages)
                .ThenInclude(m => m.User)
                .OrderBy(m => m.CreatedAt)
                .ToList();

            ThreadWrapper wrapper = new ThreadWrapper()
            {
                LoggedInUser = loggedInUser,
                Threads = threads
            };

            return View("threads", wrapper);
        }

        [HttpGet("/newthread")]
        public IActionResult NewThread()
        {
             // make sure user is logged in
            string userEmail = HttpContext.Session.GetString("logged_in_user");

            if (userEmail == null)
            {
                return RedirectToAction("Index");
            }

            User loggedInUser = _context.Users
                .Where(u => u.Email == userEmail)
                .SingleOrDefault();

            return View("newthread", loggedInUser);
        }

        [HttpPost("/newthread")]
        public IActionResult NewThread(string threadTitle, string newTextBody)
        {
            string userEmail = HttpContext.Session.GetString("logged_in_user");

            if (userEmail == null)
            {
                return RedirectToAction("Index");
            }

            User loggedInUser = _context.Users
                .Where(u => u.Email == userEmail)
                .Include(u => u.Messages)
                .SingleOrDefault();


            if (loggedInUser.Messages == null)
            {
                loggedInUser.Messages = new List<Message>();
            }

            if (string.IsNullOrEmpty(threadTitle))
            {
                ViewBag.Error = "Title must not be empty!";
                return View("newthread", loggedInUser);
            }

            if (string.IsNullOrEmpty(newTextBody))
            {
                ViewBag.Error = "Thread text must not be empty!";
                return View("newthread", loggedInUser);
            }

            Thread newThread = new Thread()
            {
                Title = threadTitle,
                UserId = loggedInUser.UserId,
                User = loggedInUser,
                Messages = new List<Message>()
            };

            Message newMessage = new Message()
            {
                UserId = loggedInUser.UserId,
                User = loggedInUser,
                Text = newTextBody,
            };

            loggedInUser.Messages.Add(newMessage);
            newThread.Messages.Add(newMessage);

            _context.Add(newThread);

            _context.SaveChanges();

            return RedirectToAction("ViewThread", new { threadId = newThread.ThreadId });
        }

        [HttpGet("/thread/{threadId}")]
        public IActionResult ViewThread(int threadId)
        {
            string userEmail = HttpContext.Session.GetString("logged_in_user");

            if (userEmail == null)
            {
                return RedirectToAction("Index");
            }

            User loggedInUser = _context.Users
                .Where(u => u.Email == userEmail)
                .Include(u => u.Messages)
                .SingleOrDefault();

            Thread thread = _context.Threads
                .Where(t => t.ThreadId == threadId)
                .Include(t => t.Messages)
                .ThenInclude(m => m.User)
                .SingleOrDefault();

            // order thread messages by their create date
            thread.Messages = thread.Messages.OrderBy(m => m.CreatedAt).ToList();

            SingleThreadWrapper wrapper = new SingleThreadWrapper()
            {
                LoggedInUser = loggedInUser,
                Thread = thread
            };

            return View("thread", wrapper);

        }

        [HttpPost("/postmessage")]
        public IActionResult PostMessage(int threadId, string newMessageText)
        {
            string userEmail = HttpContext.Session.GetString("logged_in_user");

            if (userEmail == null)
            {
                return RedirectToAction("Index");
            }

            User loggedInUser = _context.Users
                .Where(u => u.Email == userEmail)
                .Include(u => u.Messages)
                .SingleOrDefault();

            Thread thread = _context.Threads
                .Where(t => t.ThreadId == threadId)
                .Include(t => t.Messages)
                .SingleOrDefault();

            Message newMessage = new Message()
            {
                UserId = loggedInUser.UserId,
                User = loggedInUser,
                Text = newMessageText,
            };

            loggedInUser.Messages.Add(newMessage);
            thread.Messages.Add(newMessage);

            _context.Add(newMessage);

            _context.SaveChanges();

            return RedirectToAction("ViewThread", new { threadId = thread.ThreadId });
        }

        [HttpGet("/user/{userId}")]
        public IActionResult UserDetail(int userId)
        {
            string userEmail = HttpContext.Session.GetString("logged_in_user");

            if (userEmail == null)
            {
                return RedirectToAction("Index");
            }

            User loggedInUser = _context.Users
                .Where(u => u.Email == userEmail)
                .Include(u => u.Messages)
                .SingleOrDefault();

            User detailUser = _context.Users
                .Where(u => u.UserId == userId)
                .Include(u => u.Messages)
                .ThenInclude(m => m.Thread)
                .SingleOrDefault();

            int totalMessages = detailUser.Messages.Count;

            detailUser.Messages = detailUser.Messages.OrderByDescending(m => m.CreatedAt).Take(4).ToList();

            UserPageWrapper wrapper = new UserPageWrapper()
            {
                LoggedInUser = loggedInUser,
                DetailUser = detailUser,
                TotalPostsForDetailUser = totalMessages
            };

            return View("userpage", wrapper);
        }

    }
}