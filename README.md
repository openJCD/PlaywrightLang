# PlaywrightLang - a scripting language for theatre kids
### Playwright is a work-in-progress A Level EPQ project. Updates are frequent, but don't expect too much!  
## Goals:
### 1: Create an embeddable scripting language (in the same vein as Lua)
### 2: Showcase it in a game context
### 3: Have its syntax be as human-readable and close to a real play script as possible

# Features:
- Close integration with C#
  - Class members in C# can be annotated with `PwItemAttribute` to register them with the given name inside of Playwright.
  - These classes can be registered as types by the state as long as they inherit from `PwObjectClass`
- First-class functions
- Dynamic types

# Notable flaws:
- Extremely slow due to the tree-walking implementation
  - Version 2, which will be a bytecode interpreter, is planned to solve this.
- Limited type system
  - Only one numeric type (may change in future)
  - Automatic conversions are not supported. 
