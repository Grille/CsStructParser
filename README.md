# CsStructParser<br>
structs parser developed for city game in C#<br>

## Features
parse simple human-readable structs from files.<br>
inheritance of structures and arrays<br>

## Use in C#
````cs
Parser parser = new Parser();
parser.ParseFile("code.txt");
foreach (string obj in parser.ObjectNames)
{
  Write(obj);
  foreach (string att in parser.AttributeNames)
  {
    Write(parser.GetAttribute<string>(obj, att));
  }
}
````
## "code.txt" Demo
````cs
//type and name of the attributes
Attributes{
  string name;
  int number = 42;
  int[] array;
}

//set values
<obj0>{
  name = "obj-0";
  array = [1,2]; //set values
}
<obj1>:obj0 { //inherits from obj0
  name = "obj-1"; 
  array + [3,4]; //add values
}
````
## Output
````c
obj0:
-name "obj-0"
-number 42 //default
-array [1,2]
obj1:
-name "obj-1"
-number 42 //inherited from obj0
-array [1,2,3,4] //add obj0.array values to obj1.array values
````