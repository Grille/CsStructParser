# GameData-Parser<br>
structs parser developed for city game in C#<br>

## Features
read simple human-readable structs from files.<br>
inheritance of structures and arrays<br>

## Use in C#
````cs
Parser parser = new Parser();
parser.ParseFile("code.txt");
foreach (string obj in parser.GetObjectNames())
{
    parser.GetAttribute<string>(obj, "name");
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
  array = [];
}
````
set values<br>
````js
<0>{
  name = "obj-0";
  array = [1,2]; 
}
<1>:0 { //inherits from <0>
  name = "obj-1"; 
  array + [3,4]; 
}
````
results<br>
````js
0:
  name = "obj-0"; 
  number = 42; //from init
  array = [1,2]; 
1:
  name = "obj-1"; 
  number = 42;//inherited from <0>
  array = [1,2,3,4]; //add values from <0> & <1>
````