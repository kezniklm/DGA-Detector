﻿name: .NET

on:
push:
branches: [ "main" ]
pull_request:
branches: [ "main" ]

jobs:
build:
runs - on: ubuntu - latest

steps:
-uses: actions / checkout@v4
    - name: Setup.NET
uses: actions / setup - dotnet@v4
with:
dotnet - version: '8.0.x'
    - name: Restore dependencies
run: dotnet restore
working-directory: src / Web application /
    -name: Build
run: dotnet build --no-restore
working-directory: src / Web application /
    -name: Test
run: dotnet test --no-build --verbosity normal
working-directory: src / Web application /
