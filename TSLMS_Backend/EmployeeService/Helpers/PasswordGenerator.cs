using System;
using System.Linq;

namespace EmployeeService.Helpers
{
    public static class PasswordGenerator
    {
        private static readonly string _chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";

        public static string Generate(int length = 10)
        {
            var random = new Random();
            return new string(Enumerable.Repeat(_chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}