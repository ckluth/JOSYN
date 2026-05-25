# session opener prompt for session-019 on the story "poc".

## What to do

Grooming all the XML-Comments over the whole repo 

## Basic instructions

- re-check all the xml-comments thoroughly. they are currently a mess. all in the range between perfect and empty or "TODO". maybe some are already outdated.
- the comments shall be entirely in english. 
- the comments shall provide good hints for the cusomizing developer. no academic over-documenting. 
- a good <summary> tag is of course a must. feel free to skip no-brainers for parameters and return-value. 
- <remarks> only if really helpful. 
- prefer always the pattern: comment in the contract. implementation relies on <inheritdoc/>  

## Targets

### NuGet-Artifacts

- main targets are the projects which produce NuGet-Artifacts
- make sure, that every nuget-csproj contains this setting: "<GenerateDocumentationFile>true</GenerateDocumentationFile>" 
- within the main targets, all public members are a must (otherwise warnings)
- feel free, to document internal members also, if you consider this really relevant for the maintainer-team-members. but be thrifty there! 

### Other Artifacts
- treat all no-nuget-projects in a reasonable way!
- treat public and internal members like above described: if you consider it with high relevance for the maintainer-team-members, to understan a not-obvious context, than document it. otherwise: no.
- no "<GenerateDocumentationFile>true</GenerateDocumentationFile>" in the csproj-file here.

### Everywhere
- Interfaces always have to be XML-documented, regardless of the context.

## How to process

- make a two-pass-run. 
- first make a plan for yourself and find possible candidates, you cannot comment without more infoarmation. 
- any members, which are not really clear in their purpose shall be reported and discussed with the human, before you continue.
- when anything is clear, you can go autopilot and have my allowance for all necessary operations.

## Artifacts

### Code-Work

- all code updated
- all compiling
- all tests green
- all pushed to to remote

### Meta-Work

- a new entry in the repo "copilot-instructions" fur future re-use of these instructions on XML-Comments 
- a brief summary of the whole session in the session-results









