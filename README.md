# KH Package Core

This contains scripts that I have that don't depend on proprietary addons. This is publicly available just so I can share my projects without having to share a github PAT.

**WARNING: I make no guarantees about any of this code, and breaking changes will be pushed without any regard to breaking other's people's projects (or my own). If you want to use anything in this, you should either copy the files directly into your project or add the dependency and not update.**

If there's anything that you think is uniquely interesting and deserves its own library with a bit more of a guarantee (like my menu generation code, Menutee, [here](https://github.com/khutchins/menutee)) file an issue and I'll consider splitting it out.

This depends on Menutee for the note and object reference menu add-ons, so that'll have to be installed as well if you want to use the whole package in your project.

## Installation

### Modify manifest.json

Add these to the list of dependencies:

```
"com.khutchins.package.core": "https://github.com/khutchins/kh-package.git",
"com.khutchins.menutee": "https://github.com/khutchins/menutee.git",
"com.khutchins.ratferences": "https://github.com/khutchins/ratferences.git",
```
