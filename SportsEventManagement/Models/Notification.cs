﻿using System;

namespace SportsEventManagement.Models
{
    public class Notification
    {
        public int Id { get; set; } 
        public int UserId { get; set; } 
        public User User { get; set; }
        public string Message { get; set; } 
        public string? Link { get; set; }
        public bool IsRead { get; set; } = false; 
        public DateTime CreatedAt { get; set; } = DateTime.Now; 
        public DateTime NotificationDate { get; set; }
    }
}
