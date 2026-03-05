# ScriptableObject Drawer

This package adds two inspector conveniences for ScriptableObject fields: quick creation and assignment, and in-place editing.

## Creating and Assigning ScriptableObjects

![EditPopup.png](Documentation%7E/CreateScriptableObjectPopup.png)

When the field is empty, the create and assign buttons will be shown.

You can create a new object with the `+` button. When your project contains multiple derived classes from the type defined on the field, you will be prompted to select one of the derived types. When only one type is available, it will be automatically selected and the prompt will be skipped.

The assign button looks in the project for existing objects of the same type, or derived types, and tries to select the best match based on some usage and access heuristics. This works really well for types of which you typically only have one instance in your project.

## In-Place Editing

![EditPopup.png](Documentation%7E/EditPopup.png)

For fields with an assigned object an in-place edit button will be shown. Clicking it you shows a popup that renders the editor for the selected object in place. 

The popup supports nesting up to four levels deep (this limit is annoyingly the result of how Unity manages the popups). To lock the object editor in a new, dockable window you can use the lock button in the popup's title bar. If you reach the maximum nesting level, the drawer will also open the editor such a window.

## Installation

Unity Package Manager; open the Package Manager and select `Add package from git URL`:

```
https://github.com/besttof/ScriptableObjectDrawer.git
```
