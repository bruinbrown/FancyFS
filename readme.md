# FancyFS
## What's Fancy?
Fancy is a web server built in F# which is intended to be super easy to use.

## How does it work?
A Fancy application consists of a series of functions chained together in order to create a pipeline. Each function takes a request and a response and returns a new response. So we get a type signature for each element like:
`Request -> Response -> Repsonse
Super easy is the aim.

## How can it be hosted?
The aim is to host it through either OWIN or on IIS initially with more the potential for more options in the future.

## Current progress?
Very early alpha. Not production ready yet.
