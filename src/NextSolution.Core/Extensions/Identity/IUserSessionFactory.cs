﻿using NextSolution.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Extensions.Identity
{
    public interface IUserSessionFactory
    {
        Task<UserSessionInfo> GenerateAsync(User user);

        Task<bool> ValidateAccessTokenAsync(string accessToken);

        Task<bool> ValidateRefreshTokenAsync(string refreshToken);
    }
}
