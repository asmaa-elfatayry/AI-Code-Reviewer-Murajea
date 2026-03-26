using System;


    public class UserService
    {
        private readonly List<User> _users = new();

        // دي دالة فيها مشاكل عشان الأداة تكتشفها
        public User CreateUser(string name, string email)
        {
            // مشكلة: مفيش validation للـ email
            // مشكلة: مفيش null check للـ name
            var user = new User
            {
                Name = name,
                Email = email,
                CreatedAt = DateTime.Now
            };

            _users.Add(user);
            return user;
        }

        public User GetUserById(int id)
        {
            // مشكلة: ممكن ترجع null من غير ما تتحقق
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public async Task<bool> UpdateUserEmailAsync(int id, string newEmail)
        {
            // مشكلة: async method من غير CancellationToken
            var user = GetUserById(id);
            if (user == null) return false;

            user.Email = newEmail;
            await Task.Delay(100);
            return true;
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

