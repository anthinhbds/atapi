using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using atmnr_api.Models;
using atmnr_api.Entities;

namespace atmnr_api.Authorization
{
    public interface IAuthReposity
    {
        Task<UserResultInfo> Login(string username, string password);
        Task<UserResultInfo> Logout(string token);
        Task<WrapModel<string>> Register(User user, string password);
        Task<string> ChangePassword(ChangePasswordModel model);
    }
}