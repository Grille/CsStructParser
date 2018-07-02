# GameData-Parser<br>
gd parser developed for city game in C#<br>

## Features
write/read arrays of structs to/from files<br>
inheritance of structures<br>

## Use in C#
````cs
 //.
````
## GD-format
type and name of the attributes<br>
````
 Attributes
 {
  string name;
  int number;
  int[] array;
 }
````
default values for attributes<br>
````
 Init
 {
  name = "";
  number = 0;
  canBuiltOn = [];
}
````
set values<br>
````
 ID-0 {
  name = "obj0"; 
  array = [1,2]; 
 }
 ID-1:0 {
  name = "obj1"; 
  number = 1;
 }
 ID-2 {
  name = "obj2"; 
  number = 2;
 }
````
results<br>
````
 0:
  name = "obj0"; 
  number = 0;
  array = [1,2]; 
 1:
  name = "obj1"; 
  number = 1;
  array = [1,2]; 
 2:
  name = "obj2"; 
  number = 2;
  array = []; 
````