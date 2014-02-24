# FancyFS
## What's Fancy?
Fancy is a web server built in F# which is intended to be super easy to use.

## How does it work?
A Fancy application consists of a series of functions chained together in order to create a pipeline. Each function takes a request and a response and returns a new response. So we get a type signature for each element like:  
```Async<(Request * Response)> -> Async<(Request * Response)>```  
Super easy is the aim.

## What's included?
The aim of Fancy is to try and mirror some concepts from both Sinatra(Ruby) and Nancy(C#) whilst at the same time using more idiomatic F# and making the most of some of the awesome features of F#

## How can it be hosted?
The aim is to host it through either OWIN or on IIS initially with more the potential for more options in the future.

## Current progress?
Very early alpha. Not production ready yet.
