﻿using System.Security.Claims;

namespace NextSolution.Server.Models.Identity
{
    public class SignInWithForm 
    {
        public SignInProvider Provider { get; set; }

        public string ProviderKey { get; set; } = null!;

        public string? ProviderDisplayName { get; set; }

        public ClaimsPrincipal Principal { get; set; } = null!;
    }
}
