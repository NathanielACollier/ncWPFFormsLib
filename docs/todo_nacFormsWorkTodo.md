# nac.forms Avalonia Features Missing from WPF Version

## Summary

This document identifies all features implemented in the [Avalonia](https://docs.microsoft.com/avalonia-ui/) version of `nac.forms` that are missing from the [WPF](https://learn.microsoft.com/wpf/) version located at `C:\sc_git\dotnet\ncWPFFormsLib`.

---

## **CRITICAL MISSING FEATURE: ViewModel DataContext Binding**

### What Avalonia Has:

The Avalonia version includes a dedicated `DataContext` property on the `Form` class that can accept either:
1. A `nac.utilities.BindableDynamicDictionary` (dictionary binding)
2. Any object implementing `INotifyPropertyChanged` (ViewModelBase or similar classes)

Key files:
- [`Form.cs`](C:\sc_git\dotnet\dotnetCoreAvaloniaNCForms\nac.Forms\Form.cs) Lines 28-32 - DataContext property getter/setter
- [`Form.DataContext.cs`](C:\sc_git\dotnet\dotnetCoreAvaloniaNCForms\nac.Forms\Form.DataContext.cs) - All binding infrastructure

### How It Works:

```csharp
// Avalonia version allows both patterns:

// Pattern 1: Dictionary binding (like WPF currently supports)
var form = new Form();
form.Model["Name"] = "John";
form.TextBoxFor("name");

// Pattern 2: ViewModel-based binding (MISSING from WPF)
public class MyViewModel : INotifyPropertyChanged
{
    public string Name { get; set; }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

var form = new Form();
form.DataContext = new MyViewModel(); // NO ERROR - this works in Avalonia!


// All controls bind to the DataContext object properties
form.TextBoxFor("name");  // Automatically reads MyViewModel.Name property

// Watch for changes
f.Model["Name"] = "Jane";   // Updates UI automatically via PropertyChanged


```

### Why This Is Important:

- Most MVVM applications already have ViewModels with INotifyPropertyChanged
- Allows reusing existing ViewModel code in forms
- No need to manually sync Model dictionary with ViewModel properties
- Matches modern WPF development practices (ViewModels + INotifyPropertyChanged)

---

## **MISSING FUNCTIONALITY LIST**

After comparing the WPF version (`ncWPFFormsLib`) with Avalonia version (`dotnetCoreAvaloniaNCForms`), here are the features to add:

### 1. ❌ DataContext Property for ViewModel Binding (CRITICAL)

**Current WPF State:**
```csharp
public nac.utilities.BindableDynamicDictionary Model { get; set; }
```

**What Avalonia Has:**
```csharp
public object DataContext { ... }
```

**How Avalonia Implements It:**
Located in [`Form.DataContext.cs`](C:\sc_git\dotnet\dotnetCoreAvaloniaNCForms\nac.Forms\Form.DataContext.cs):
- `getBindingSource()` - Determines which binding source to use (DataContext or Model)
- `notifyOnRootModelChange()` - Sets up change notifications for ViewModel properties
- `getDataContextValue()` - Navigates property paths in either ViewModel or Dictionary
- `setDataContextValue()` - Updates ViewModel properties by name/path
- `getModelValue()` / `setModelValue()` - Convenience methods to read/write ViewModel

**Action Required:**
Port all of `Form.DataContext.cs` to the WPF project and integrate with existing Form.cs.

---

### 2. ✅ All Control Types (Already Present)

Both versions have:
- TextBox, Label, PasswordBox, CheckBox
- DropDown/ComboBox, Date picker  
- Button with lambda click handlers
- File/Directory pickers
- Busy indicators (Avalonia uses ElipseSpinnerControl)
- AutoSuggest single/multiple selection
- Table/List controls with templates
- TreeView Object Viewer

---

### 3. ✅ Layout System (Comparable Features)

**WPF Version Has:**
- HorizontalGroup / VerticalGroup with splitters
- HorizontalStack / VerticalStack

**Avalonia Version Adds:**
- `VerticalDock` - Uses DockPanel instead of Grid
- `HorizontalDock` - Missing from WPF version but exists in Avalonia

**Action Required:**
Either port to DockPanel or add VerticalDock/HorizontalDock as additional options.

---

### 4. ✅ Tab Support (Feature Parity)

Both versions have:
- `Tabs()` with array of TabCreationInfo
- Custom header support via PopulateHeader callback  
- Shared model across tabs
- Tab creation lambdas via delegate params

---

### 5. ✅ Object Viewer (Comparable API)

**Avalonia:**
```csharp
await form.ObjectViewer<T>(initialItemValue, functions);
```

**WPF Version:** Already has this in [`Form.Controls.ObjectViewer.cs`](C:\sc_git\dotnet\ncWPFFormsLib\nac.wpf.forms\Form.Controls.ObjectViewer.cs)

---

### 6. ✅ Table Support with Custom Columns

Both have:
- `Table(modelFieldName, columns)` 
- Column class with Header, modelBindingPropertyName, template
- Template support for full layout control

**Note:** Avalonia uses `Panel` method for content panels with FuncDataTemplate

---

### 7. ✅ File System Dialogs

Both have:
- `FilePathFor()` with file filtering
- `DirectoryPathFor()` for folder browsing

---

## **SOLUTION FOR DataContext Binding (ViewModel Support)**

### Implementation Plan:

Create a new partial class `Form.DataContext.cs` in the WPF project at:
```
C:\sc_git\dotnet\ncWPFFormsLib\nac.wpf.forms\Form.DataContext.cs
```

**File Contents:**

The core logic needed is what's currently in [`Form.DataContext.cs`](C:\sc_git\dotnet\dotnetCoreAvaloniaNCForms\nac.Forms\Form.DataContext.cs):

1. Add a `DataContext` property:
   ```csharp
   public object DataContext { 
       get => this.Model[specialKeys.DataContext]; 
       set => this.Model[specialKeys.DataContext] = value; 
   }
   ```

2. Implement `getBindingSource()` method that returns either:
   - The DataContext if it's INotifyPropertyChanged
   - Falls back to Model dictionary

3. Add helper methods for navigating paths (`getDataContextValue`, `setDataContextValue`)

4. Wrap property change notifications in watchers (`notifyOnModelChange`)

5. Create the `DataContextValueResult` class to track path navigation and value retrieval

### Minimal Changes Needed:

The implementation can reuse:
- [`model.ModelFieldNamePathInfo`](C:\sc_git\dotnet\dotnetCoreAvaloniaNCForms\nac.Forms\model\ModelFieldNamePathInfo.cs) - Split property paths
- Existing `BindableDynamicDictionary` from utilities library
- Existing INotifyPropertyChanged pattern

Only need to write ~120 lines of code including:
- DataContext property (5 lines)
- getBindingSource() method  
- WatchPathForModelChanges() recursive walker (30 lines)
- getDataContextValue() value retrier (40 lines)
- setDataContextValue() path setter (35 lines)
- ModelFieldNamePathInfo class (already exists in utilities?)

---

## **Other Minor Differences**

### Avalonia Has Extra Layout Options:
- `VerticalDock()` using DockPanel (not in WPF version yet, but Grid is equivalent)
- `HorizontalStack()` and `VerticalStack()` exist in both

### Command Syntax Differences:
WPF uses lambdas like:
```csharp
.ButtonWithLabel("Click", args => { /* click handler */ })
```

Avalonia appears to use more delegate-focused patterns but the end result is similar.

---

## **Priority List**

1. **[CRITICAL]** Implement DataContext property for ViewModel binding
2. **[HIGH]** Review table implementation differences (Panel vs Grid/Table)
3. **[MEDIUM]** Add vertical docking support if needed
4. **LOW** Any minor API improvements from Avalonia tests/examples

---

## Files to Copy/Port

From `C:\sc_git\dotnet\dotnetCoreAvaloniaNCForms\nac.Forms`:
- Form.DataContext.cs (entire file)
- model\ModelFieldNamePathInfo.cs (might need in WPF utilities if missing)

---

## Testing Notes

The Avalonia tests at `Tests/FormTests.cs` demonstrate ViewModel-based usage. After implementing DataContext binding:

1. Create test ViewModel class inheriting from INotifyPropertyChanged  
2. Set form.DataContext to that ViewModel
3. Verify TextBoxFor() reads property values
4. Modify model property and verify UI updates automatically

---

## Documentation Update Needed

Once DataContext binding is implemented, update the WPF docs at `docs/AGENTS_Readme.md` to include this as a new capability.
