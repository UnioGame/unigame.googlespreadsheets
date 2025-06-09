
# Unity3D Google Spreadsheets v.4 Support

Unity3D Google Spreadsheet export/import library

**Odin Inspector Asset recommended to usage with this Package (https://odininspector.com)**

# Installation

Add to your project manifiest by path [%UnityProject%]/Packages/manifiest.json new dependency:

```json
  "dependencies": {
        "com.unity.addressables": "2.6.0",
        "com.unigame.addressablestools" : "https://github.com/UnioGame/unigame.addressables",
        "com.unigame.typeconverters" : "https://github.com/UnioGame/unigame.typeconverters.git",
        "com.unigame.unicore": "https://github.com/UnioGame/unigame.core.git",
        "com.unigame.localization": "https://github.com/UnioGame/unigame.localization.git",
        "com.unigame.rx": "https://github.com/UnioGame/unigame.rx.git",
        "com.cysharp.unitask" : "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
        "com.unigame.google.api.v4" :  "https://github.com/UnioGame/google.api.v4.git",
        "com.unity.nuget.newtonsoft-json": "3.0.2"
}
```

## Features

- Export Serializable Project Data into Google Spreadsheet
- Support Custom Import/Export data providers
- Support nested synched spreadsheed fields
- Support export/import JSON to serializable classed
- Export/import into Addressables AssetReferences
- Export/import into Unity Scriptable Objects
- Export/import base Unity Assets types

## Table of Content

- [Data Definitions](#data-definitions)
- [Nested Spreadsheet tables](#nested-spreadsheet-tables)
- [Connect to Google Spreadsheet](#connect-to-google-spreadsheet)
- [Supported types](#supported-types)

## Create Importer asset

Menu: "UniGame/Google Spreadsheet/Google Spreadsheet Asset"

![](https://github.com/UnioGame/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/create_spreadsheet_asset.png)

## Data Definitions

### Import and Export Attributes

=============

**SpreadsheetTargetAttribute**

Allow to mark serializable class as Synched with Google Spreadsheet. 

**SpreadsheetTargetAttribute(string sheetName = "",string keyField = "",bool syncAllFields = true)**

Parameters:
- sheetName. Name of target Table into Google Spreadsheet. If **sheetName** empty, then class Type name will be used as SheetName
- keyField. Primary key field name for sync between spreadsheet and serialized class 
- syncAllFields. If TRUE, then try to sync all class fields data. If FALSE - synchronized only fields marked with attribute **[SpreadSheetFieldAttribute]**

=============

**SpreadSheetFieldAttribute**

All fields marked with this attribute will be synchronized with target Spreadsheet data

**public SpreadSheetFieldAttribute(string sheetField = "", bool isKey = false)**

Parameters:
- sheetField. Name of target Spreadsheet column
- isKey. If TRUE, then target field will be used as Primary Key field.

```csharp

[SpreadsheetTarget("DemoTable")]
public class DemoSO : ScriptableObject{

    [SpreadSheetField("KeyField",true)]
    public string key;

    [SpreadSheetField("ValueField",true)]
    [SerializableField]
    private string value;

}

```
![](https://github.com/UniGameTeam/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/sheet_fields.png)

=============

Default Sheet Id

```csharp

[SpreadSheetField("DemoTable",syncAllFields: true)]
public class DemoSO : ScriptableObject{

    public string id; // Field with name Id | _id | ID will be used as Primary key by Default

    private string value; // syncAllFields value active. Import/Export try to find Sheet column with name "Value"

}

```

=============

## Nested Spreadsheet tables

Nested fields support

```csharp

[SpreadSheetField("DemoTable",syncAllFields: true)]
public class DemoSO : ScriptableObject{

    public string id; // Field with name Id | _id | ID will be used as Primary key by Default

    [SpreadSheetField("ResourcesTable",syncAllFields: true)]
    private int cost; // syncAllFields value active. Import/Export try to find column with name "Cost" from sheet "ResourcesTable" 

}

```


![](https://github.com/UniGameTeam/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/nested_sheet.png)

Nested Table

![](https://github.com/UniGameTeam/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/nested_table_field.png)

## Supported Types

Spreadsheet library support all base value types. 

### Custom Type support

To add support your custom type you need to realize new converter that extends base interface ITypeConverter. As a shortcut you can implement BaseTypeConverter class and mark with [Serializable] attribute.

```csharp
public interface ITypeConverter
{
    bool CanConvert(Type fromType, Type toType);
    TypeConverterResult TryConvert(object source, Type target);
}
```

```csharp
[Serializable]
public class StringToVectorTypeConverter : BaseTypeConverter
{
    ....
}
```

Now we can add it into converters list

![](https://github.com/UnioGame/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/typeconvert1.png)

### Unity Assets Support

your can make reference to unity asset by type filtering and specifying name of asset

```csharp

[SpreadSheetField("DemoTable",syncAllFields: true)]
public class DemoSO : ScriptableObject{

    public string id; // Field with name Id | _id | ID will be used as Primary key by Default

    private Sprite iconAsset; 

}

```

![](https://github.com/UniGameTeam/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/asset_ref1.png)

#### Auto Sync Scriptable Assets on Import

![](https://github.com/UnioGame/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/spreadsheet_asset_creation.gif)

### Unity Addressables References support

Same usage as regular unity asset type with only one exception. The type of target field must be AssetReference or inherited from AssetReference 

```csharp

[SpreadSheetField("DemoTable",syncAllFields: true)]
public class DemoSO : ScriptableObject{

    public string id; // Field with name Id | _id | ID will be used as Primary key by Default

    private AssetReference iconReference; 

}

```


![](https://github.com/UniGameTeam/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/asset_ref2.png)

### JSON support

For more complex scenarios JSON serialization can be used

```csharp

[SpreadSheetField("DemoTable",syncAllFields: true)]
public class DemoSO : ScriptableObject{

    public string id; // Field with name Id | _id | ID will be used as Primary key by Default

    [SpreadSheetField("ItemsTable",syncAllFields: true)]
    private ItemData defnition; // sync item data from json value value

}

[Serializable]
public class ItemData
{
    public string id;
    
    public int position;
}


```

![](https://github.com/UniGameTeam/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/json_support1.png)

![](https://github.com/UniGameTeam/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/json_support2.png)

## Pipeline Importer

for more complex import scenario we can use **Pipeline Importer** Asset

### How To Create

Create Menu: **"UniGame/Google/Importers/PipelineImporter"**

Each step of import pipeline take data from previous one. For custom import step your should implement one of:

- SerializableSpreadsheetImporter: pure serializable c# class
- BaseSpreadsheetImporter: ScriptableObject asset

For example:

- Create Units Prefabs by Spreadsheet Data
- Apply Unit settings from sheets data to assets
- Mark Assets as an Addressables an move them into target group

![](https://github.com/UnioGame/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/webapp5.png)

![](https://github.com/UnioGame/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/Import%20Characters%20from%20Spreadsheets1.gif)


## Connect to Google Spreadsheet

### Editor Window

![](https://github.com/UniGameTeam/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/menu.png)


### Create Google Api Credentials


- HOWTO create api credentials https://developers.google.com/sheets/api/quickstart/dotnet

**IMPORTANT**

When you create an Desktop API KEY:

![](https://github.com/UnioGame/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/webapp2.png)

![](https://github.com/UnioGame/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/webapp3.png)

- Setup path to credential .json file and press "Connect Spreadsheet" 

![](https://github.com/UniGameTeam/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/editorapikey.png)

- Under target Google Profile allow application access to spreadsheets

![](https://github.com/UniGameTeam/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/editorapikey2.png)


### Spreadsheet Id's

- Now you can specify your spreadsheets

![](https://github.com/UniGameTeam/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/editor.png)

- Id's of your sheet can be found right from web page url

![](https://github.com/UniGameTeam/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/sheetid.png)

- Copy your table id and paste into importer window field

![](https://github.com/UniGameTeam/UniGame.GoogleSpreadsheetsImporter/blob/master/GitAssets/sheetid1.png)


## Google API V4 .NET References

- https://developers.google.com/sheets/api/quickstart/dotnet

- https://googleapis.dev/dotnet/Google.Apis.Sheets.v4/latest/api/Google.Apis.Sheets.v4.html

## UPM Installation

Add to your project manifiest by path [%UnityProject%]/Packages/manifiest.json new dependency:

```json
{
  "dependencies": {
    "unigame.unityspreadsheets" : "https://github.com/UnioGame/UniGame.GoogleSpreadsheetsImporter.git",
  }
}

```

### Official Newtonsoft.Json Unity Package

```json
{
  "name": "com.unity.nuget.newtonsoft-json",
  "displayName": "Newtonsoft Json",
  "version": "2.0.0",
  "unity": "2018.4",
  "description": "Newtonsoft Json for use in Unity projects and Unity packages. Currently synced to version 12.0.2.\n\nThis package is used for advanced json serialization and deserialization. Most Unity users will be better suited using the existing json tools built into Unity.\nTo avoid assembly clashes, please use this package if you intend to use Newtonsoft Json.",
  "type": "library",
  "repository": {
    "type": "git",
    "url": "git@github.cds.internal.unity3d.com:unity/com.unity.nuget.newtonsoft-json.git",
    "revision": "74ca86c283a2f63ba5b687451a0842ba924da907"
  }
}
```

## Co-Processors

![2020-11-02_10-25-22](https://user-images.githubusercontent.com/26055406/97841423-d837cc00-1cf6-11eb-867f-5d53f3493664.png)

Selected co-processor execute after main import processor (after parsing and applying every row).


**Custom co-processor**

```csharp

[Serializable]
public class MyCustomCoProcessor : ICoProcessorHandle
{
    // some properties
    
    public void Apply(SheetValueInfo valueInfo, DataRow row)
    {
        // some code
    }
}

```

**Example**

For example, Nested Table Co-Processor applies nested google-table where filter is parsing pattern:

![2020-11-02_10-25-59](https://user-images.githubusercontent.com/26055406/97841997-f651fc00-1cf7-11eb-8fa8-580fd9504fad.png)

![2020-11-02_10-26-44](https://user-images.githubusercontent.com/26055406/97842017-fe11a080-1cf7-11eb-8b30-320bdb31282a.png)
