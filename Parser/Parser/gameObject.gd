


 Attributes {
  string name;
  int value;
  bool CanBeNegative;
  bool storable;
  string[] array;
 }

 Init {
  value = 0;
  array = ["k1 "];
 }

 ID-0 {
  value = 200000;
  name = "Money";
  array + ["tach"];
  //storable = true;
  //CanBeNegative = true;
 }

 ID-1 {
  name = "energy";
  //array = [1,1];
  array = ["aa","bb"];
 }

 ID-2:1 {
  name = "water";
  array + ["cc"];
 }

 ID-3 {
  name = "waste";
  value = 2000;
 }
