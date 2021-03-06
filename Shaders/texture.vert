﻿#version 330 core

layout (location = 0) in vec3 in_Position;
layout (location = 1) in vec3 in_Normal;
layout (location = 2) in vec2 in_TexCoord;
layout (location = 3) in vec4 in_Color;

out vec2 texCoord;

void main()
{
	gl_Position = vec4(in_Position, 1.0);
	texCoord = in_TexCoord;
}