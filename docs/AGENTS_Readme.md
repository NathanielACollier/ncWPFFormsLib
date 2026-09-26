# nac.wpf.forms - API Documentation and Extensibility Guide

## Architecture Overview

A fluent builder library for building WPF GUI applications quickly using a model-driven approach similar to React/Vue data binding but with WPF controls.

### Core Concepts

- **Form**: Main container that manages WPF hosting infrastructure, provides fluent API
- **Model**: `BindableDynamicDictionary` - Shared observable data context supporting MVVM-style bindings
- **Host**: `NacFormHostGrid` - Root Grid that receives all child controls
- **Nested Forms**: Child containers created via grouping methods, strips children and adds them to parent

---

## API Surface Reference

### Control Types

#### Basic Input Controls (Form.Controls.cs)

| Method | Description | Parameters |
|--------|-------------|------------|
| `TextBoxFor<T>(fieldName, value = "")` | Single-line text input | fieldName: string, value: optional initial value, onKeyUp: optional callback, multiline: bool |
| `LabelFor(fieldName, value = "")` | Display-only text (read-only binding) | fieldName: string, value: optional display text |
| `PasswordBoxFor<T>(fieldName)` | Secure password input | fieldName: string |
| `CheckBoxFor(fieldName, checkChangedAction)` | Toggle boolean | fieldName: string, checkChangedAction: callback |
| `DropDownFor(fieldName, items = null)\|enumType` | Selection dropdown | fieldName: string, items: IEnumerable or enum type |
| `DateFor(fieldName, initialDate = null)` | Date picker | fieldName: string, initialDate: DateTime? |
| `ButtonWithLabel(labelText, onClick, onControlReady = null)` | Action button with command setup | labelText: string, onClick: Action<object>, onControlReady: callback |
| `ButtonsTrueFalseFor(fieldName)` | Visual radio-style true/false toggle using StackPanel | fieldName: string |
| `Image(fieldName)` | Image display (in scrollviewer) | fieldName: string |
| `Text(text)` | Non-binding label text | text: string |
| `TextFor(fieldName)` | Bindable text display | fieldName: string |

#### File System Controls (Form.Controls.FileSystem.cs)

| Method | Description | Parameters |
|--------|-------------|------------|
| `FilePathFor<T>(fieldName, fileFilter = null, initialFileName = null, fileMustExist = true, onFilePathChanged = null, functions = null)` | Open/save file dialog | fieldName: string, fileFilter: string, initialFileName: optional default filename to show in dialog, fileMustExist: bool (only existing files), onFilePathChanged: callback, functions: FilePathForFunctions |
| `DirectoryPathFor(fieldName)` | Open folder dialog | fieldName: string |

#### Busy Indicator (Form.Controls.Busy.cs)

| Method | Description | Parameters |
|--------|-------------|------------|
| `Busy(displayText, startIsBusy = false, functions = null)` | WPF busy spinner | displayText: string, startIsBusy: bool, functions: BusyFunctions |

#### Autocomplete Controls (Form.Controls.AutoSuggest*, Form.Controls.Complex.cs)

- **AutoSuggestFor<T>**: Single-selection autocomplete using `AutoCompleteBox`
  - Items generated via lambda: string -> IEnumerable<T>
  - Supports any enum or item type T
  
- **AutoSuggestMultipleFor<T>**: Multiple selections via ListView with add/remove buttons
  - Generates items the same way for display dropdown
  
- **List(itemSourcePropertyName, populateItemRow)**: Custom-list with template support
  - Binds to collection property
  - Template function receives Form and populates row layout

- **TreeFor(fieldName, generateChildren)**: Hierarchy tree view
  - Uses `WPFTreeViewObjectPropertiesBuilder` from utilities
  - Children function receives parent node and returns IEnumerable of children
  
- **Table(itemsSourceModelName, columns = null)**: Data-bound grid/Table
  - Binds to property named itemsSourceModelName on Model
  - Optional columns array with CustomColumn objects for template support

#### Object Explorer (Form.Controls.ObjectViewer.cs)

| Method | Description | Parameters |
|--------|-------------|------------|
| `ObjectViewer<T>(initialItemValue, functions = null)` | Deep object tree viewer | initialItemValue: T, functions: ObjectViewerFunctions<T> |

The ObjectViewer displays any arbitrary object as a hierarchical tree structure using WPF TreeView. It's useful for debugging and exploring complex objects without defining custom templates.

### Layout System (Form.Layout.cs)

All groupings return `this` and accept `Action<Form>` callbacks for population:

- **`.HorizontalGroup(setupNewForm)`**: Creates horizontal Grid with equal column spacing
- **`.VerticalGroup(setupNewForm)`**: Creates vertical DockPanel (top-bottom stacking)
- **`.HorizontalGroupSplit(setupNewForm)`**: Horizontal Grid with splitters between columns  
- **`.VerticalGroupSplit(setupNewForm)`**: Vertical split containers with GridSplitter handles

All groupings support optional `isVisiblePropertyName` parameter to bind a model property for show/hide control.

### Tabs Support (Form.Tabs.cs)

```csharp
AddTab(
    Action<Form> setupNewTabForm,           // Configure the tab's content form
    string tabName = "",                     // Optional header/label text
    string tabControlIndex = "",             // Unique index for multiple tab controls
    Action<Form> populateHeaderForm = null,  // Configure specialized header
    Action OnFocus = null                    // Called when tab gains focus
)
```

Features:
- Custom header content via `populateHeaderForm` for rich tab headers using nested Form builders
- Shared model across tabs (cascading DataContext from parent form)
- OnFocus callback useful for resetting tab state on navigation

---

## Helper Methods & Utilities (Form.UI_Helpers.cs)

### Private Helpers Called Internally:

1. **Helper_setupControlCommand**: Automatically binds commands to HostGrid's DataContext, enabling command injection even in nested DataTemplates. This is key because the HostGrid serves as a single source for all command bindings.

2. **Helper_BindField**: Binds WPF control properties to model fields with specified binding mode/trigger

3. **Helper_AddRowToHost**: Adds control (optionally with label) to form's Host Grid, handles row definitions and auto-sizing

4. **addVisibilityTrigger**: Creates data triggers for show/hide controls based on model property

5. **createModelPropertyBinding**: Creates bindings to model properties

6. **SetupListViewStyleForMultipleItems**: Styles ListView items to stretch horizontally

7. **GetRelayCommands**: Finds command properties in BindableDictionary (for button activation)

### Public Methods:

- **Xaml**: Returns stringified WPF tree for debugging
- **Close()**: Closes the display window
- **BeginInvoke(Action code)**: Executes action on UI thread via Dispatcher, returns Task<bool>
- **Display(...)**: Shows form as dialog with optional width/height/onClosing/onDisplay callbacks

---

## Model Column (model/Column.cs)

Used with Table() for custom column rendering:

```csharp
class Column {
    public string Header { get; set; }
    public string modelBindingPropertyName { get; set; }
    public Action<Form> template { get; set; } // Full template customization support
}
```

- **Header**: Column heading text
- **modelBindingPropertyName**: Model property binding (string)
- **template**: Optional Action<Form> for full template customization - enables command injection, complex layouts inside table cells

---

## Extensibility Points

### 1. Adding New Controls

Steps to add a new control type:

1. Implement WPF control in `nac.wpf.controls` namespace (external dependency project)
2. Add public method to partial class `Form.cs` or create separate file under `Form.Controls.*`:
   ```csharp
   public ThisType ControlNameFor<T>(string fieldName) {
       // Create control
       var myControl = new MyCustomControl();
       
       // Optionally bind field if appropriate
       
       // Add to host via existing helper pattern
       Helper_AddRowToHost(myControl);
       
       return this;
   }
   ```
3. Return `this` for fluent API compliance
4. Use existing builder patterns (bind field names, add to host)

### 2. Adding Layout Features

Steps to add new layout type:

1. Create new partial method on `Form` class taking `Action<Form>`
2. Create nested `Form` and extract children using pattern from existing methods
3. Add to appropriate container (Grid/DockPanel/GroupBox)
4. Support visibility trigger parameter if applicable

### 3. Adding Tab Features

1. Extend `AddTab` method signature in `Form.Tabs.cs`
2. Support custom populating of other form rows via control index access (`this.controlsIndex`)
3. Maintain fluent API pattern

### 4. Adding Table Column Types

1. Add template support to existing methods
2. Store custom templates in model for later reference
3. Update Column class as needed

---

## Key Design Patterns

### Fluent Builder Pattern
All methods return `Form` or `this` enabling method chaining.

```csharp
form
    .TextBoxFor("Name")
    .ButtonWithLabel("Submit", args => { ... })
    .Display();
```

### Nested Forms as Child Containers
Nested forms created via grouping methods strip their children from their Host and add them to the parent container's layout. This allows declarative composition of complex layouts.

Example: Horizontal group passes a Form builder that becomes populated with controls added inside its callback.

### Shared Model via DataContext
Cascading construction supports shared model across parent→child→grandchild forms. Parent form creates its Model, children can optionally share the same dictionary reference for unified state management.

### Command Binding via Host Grid
The `NacFormHostGrid` serves as a single DataContext source for all command bindings. When commands are added to the model (via Model["CommandName"] = new RelayCommand(...)), they automatically bind to buttons across nested controls, even those inside DataTemplates. This is enabled by Helper_setupControlCommand which registers control elements with the Host.

### Control Indexing
Maintains `controlsIndex` dictionary tracking control references by name string for programmatic access when needed.

---

## Usage Example

```csharp
var form = new Form();
int clickCount = 0;
form
    .TextBoxFor("Status")
    .ButtonWithLabel("Click Me!", (sender, args) =>
                     {
                         form.Model["Status"] = $"Clicked {++clickCount} times";
                     })
    .Display();
```

---

## External Dependencies

- `DotNetProjects.WpfToolkit.Input` - WPF toolkit controls
- `nac.Logging` - Logging utilities
- `nac.utilities` - Core utilities (BindableDynamicDictionary, RunResult, etc.)
- `nac.wpf.Controls` - Extended WPF controls
- `nac.wpf.controls.BusyControl` - Busy indicator control

---

## Building and Publishing

The project has `<GeneratePackageOnBuild>true</GeneratePackageOnBuild>` configured. Use the provided script:

```powershell
.\PublishNugetToNugetOrg.ps1
```

See `PublishNugetToNugetOrg.ps1` for NuGet.org publishing workflow.

---

## Testing

Tests located in `/Tests/`:

- BasicFormTests.cs - Core form functionality
- TabTests.cs - Tab implementation
- ObjectViewerTests.cs - ObjectExplorer tests
- AutosuggestTests.cs - Autocomplete controls
- ListTests.cs - ListView/List controls
- MSTest_setup.cs - Test setup code

---

## Notes for Extension

When adding functionality:

1. Maintain existing code style (no comments, concise)
2. Use fluent return pattern consistently
3. Consider command injection needs in templates
4. If adding complex controls, consider external library approach
5. Keep partial class organization by feature area