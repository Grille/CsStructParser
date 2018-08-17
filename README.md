# GameData-Parser<br>
structs parser developed for city game in C#<br>

## Features
read an array of simple human-readable structs from files.<br>
inheritance of structures and arrays<br>

## Use in C#
````cs
  //string[] data;
  Parser parser = new Parser();
  parser.ParseFile("code.txt");
  for (int id = 0; id < 256; id++)
  {
      if (!parser.IDUsed(id)) continue;
      data[id] = parser.GetAttribute<string>(id, "name");
  }
````
## GD-format
type and name of the attributes<br>
````js
 Attributes
 {
  string name;
  int number;
  int[] array1, array2;
 }
````
default values for attributes<br>
````js
 Init
 {
  name = "";
  number = 42;
  array1 = [];
}
````
set values<br>
````js
 ID-0 {
  name = "obj-0"; 
  array1 = [1,2]; 
 }
 ID-1:0 { //inherits from obj0
  name = "obj-1"; 
  array1 + [3,4]; 
 }
````
results<br>
````js
 0:
  name = "obj-0"; 
  number = 42; //from init
  array1 = [1,2]; 
 1:
  name = "obj-1"; 
  number = 42;//inherited from obj0
  array1 = [1,2,3,4]; //add values from obj0 & obj1
````