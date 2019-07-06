# MediatR Caching and Cache Invalidation Behaviors

Adds per-request caching and optional invalidation using MediatR pipeline behaviors.

See [Getting Started](docs/Getting-Started.md) for more information.

## About

This library came out of the need to incorporate caching into a project that used [MediatR](https://github.com/JBogard/MediatR)
fairly extensively including validation, authorisation and logging through its inbuilt [Pipeline bevhaiors](https://github.com/jbogard/MediatR/wiki/Behaviors)
and caching seemed like a logical choice to include in the pipeline as another cross-cutting concern. As with MediatR
overall, the final solution is elegant and almost effortless to include into an existing project and also includes an
extremely powerful cache invalidation pipeline behavior to invalidate cached requests based on any other requests being
sent.

## Primary Authors and Contributors

[Daniel Lewis](https://daniellewis.dev) ([@djjlewis](https://twitter.com/djjlewis))

