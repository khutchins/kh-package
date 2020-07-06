# KH Package Core

This contains scripts that I have that don't depend on proprietary addons.

## Installation

### Create PAT

You need this to access the repo.

### Modify manifest.json

Add these lines before dependencies:

```
"scopedRegistries": [
    {
        "name": "NPM Registry",
        "url": "https://registry.npmjs.org",
        "scopes": [
            "com.mambojambostudios.unity-atoms-core",
            "com.mambojambostudios.unity-atoms-base-atoms",
            "com.mambojambostudios.unity-atoms-fsm",
            "com.mambojambostudios.unity-atoms-mobile",
            "com.mambojambostudios.unity-atoms-mono-hooks",
            "com.mambojambostudios.unity-atoms-tags",
            "com.mambojambostudios.unity-atoms-scene-mgmt",
            "com.mambojambostudios.unity-atoms-ui"
        ]
    }
],
```

That will cause the unity atoms import to proceed correctly.

Add this to the list of dependencies (adding the PAT where noted):

```
"com.khutchins.package.core": "https://<PAT>@github.com/khutchins/kh-package.git",
```
