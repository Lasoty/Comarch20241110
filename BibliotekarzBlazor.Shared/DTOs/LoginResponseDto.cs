﻿namespace BibliotekarzBlazor.Shared.DTOs;

public class LoginResponseDto
{
    public string? Token { get; set; }
    public UserDto? UserDto { get; set; }
}