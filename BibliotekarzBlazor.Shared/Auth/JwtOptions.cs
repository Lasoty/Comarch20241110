﻿namespace BibliotekarzBlazor.Shared.Auth;

public class JwtOptions
{
    public string? Key { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
}
