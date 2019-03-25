# Octave.NET [![Build Status](https://travis-ci.org/triforcely/Octave.NET.svg?branch=master)](https://travis-ci.org/triforcely/Octave.NET) [![NuGet](https://img.shields.io/nuget/dt/Octave.NET.svg)](https://www.nuget.org/packages/Octave.NET) 


![Octave.NET](https://i.imgur.com/nNGduNS.png)

📈 More than cross-platform Octave process wrapper 🔬

## Motivation

[Octave](https://www.gnu.org/software/octave/) (matlab) has excellent library of ready to use components and lets us write simpler code for solving complex
mathematical problems. This library is an attempt to bridge Octave and .NET worlds in a user-friendly and cross-platform manner while keeping single code-base and clean interface.

## Installation

  PM> `Install-Package Octave.NET`

## Usage

- Install latest octave 
- Add bin folder to system PATH variable
  or specify path to octave-cli binary in your code 
- Check the 'examples' folder

## Confirmed compatibility

| OS            | Octave | Status |
| ------------- |:--------------:| ------------:|
| Windows 10 | 4.4.0  | ✔️  |
| Ubuntu 16.04 LTS | 4.2.2       |   ✔️|


## How It's Made? 
This library spawns octave processes and controls them via standard streams (stdin, stdout and stderr). To keep optimal performance every time OctaveContext is disposed underlying octave-cli process is returned to the object pool, so we don't waste time on spawning new worker processes.
## FAQ
### How do I add octave-cli to system PATH variable?
I don't know, it varies between operating systems. Use your favourite search engine to find out.
### Is there anything I should know before using it?
In single-threaded scenario, there will be only one worker process spawned. In multithreaded scenario, library will try to supply demand for OctaveContext's by spawning more processes until the limit is reached - by default the number of logical processors in your machine. If the limit is reached and all workers are in use, calling thread will be locked until some worker is freed. You may be interested in ```OctaveContext.OctaveSettings.MaximumConcurrency``` global setting.
### What about applications that rarely use octave, are my resources wasted on idle processes?
Nope! After few seconds of inactivity (no attempts to execute octave commands) internal pool will start to slowly release its resources. 
### After a period of inactivity, first command execution takes a long time.
All processes were probably disposed and you experience "cold start". If you don't mind few more MBs that are not released until your application is closed, you can use global configuration and set ```OctaveContext.OctaveSettings.PreventColdStarts = true;```
Just remember to do this before first OctaveContext is created!
### I set some variables in octave but later when I tried to access them they were undefined!
OctaveContext is called octave context because... well, it represent single octave context at the given point of time. Even though processes are reused, it is not guaranteed that you will get the same process. Your safest bet is to do all the work in single ```using(var octave = new OctaveContext()) {...}``` block. Contexts (e.g. octave variables) will not be cleared. If you require that, just execute```clear``` command.
### XYZ function does not work in octave - I get undefined!
If your script does not work when you run it manually in octave, it won't work there. Make sure that all packages that you need are installed and loaded. 
### I try to display plot but nothing appears.
There is an 2D plot example for that. "Async" octave operations are not supported, every operation should be locking and synchronous.
