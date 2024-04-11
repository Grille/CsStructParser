# Typed Cascading config Format Parser
A parser for a format that I designed for a game project a (long) while ago.

## Features
* Strict typing
* Inheritance of properties from other objects
* Uncanny similarities to programing languages, while still being very limiting (Nesting is not possible) for its complexity.

## Use in C#
```cs
var parser = new Parser();
parser.ParseFile("code.txt");

var obj0 = parser.Result["obj0"];
var name = obj0.Get<string>("name");
var number = obj0.Get<int>("number");
var int[] = obj0.Get<int[]>("array");
```

## "code.txt" Demo
```cs
//type and name of the attributes
const HHGG = 42;

typedef Type {
  string name;
  int number = HHGG;
  int[] array;
}

//set values
Type obj0{
  name = "obj-0";
  array = [1,2]; //set values
}
Type obj1:obj0 { //inherits from obj0
  name = "obj-1"; 
  array + [3,4]; //add values
}
```

## Output
```c
obj0:
-name "obj-0"
-number 42 //default
-array [1,2]
obj1:
-name "obj-1"
-number 42 //inherited from obj0
-array [1,2,3,4] //add obj0.array values to obj1.array values
```

## Types
```cs
byte, int, float, double, bool, string
```