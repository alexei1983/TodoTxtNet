## TodoTxtNet
A library for reading and writing todo.txt files in .NET

### Introduction

The todo.txt format is an informally defined standard for representing tasks and other to-do items in a simple 
plaintext file.

Having a library to read and write todo.txt files may seem contradictory but could be useful in several circumstances.

Check out the [todo.txt page on Github](https://github.com/todotxt/todo.txt)

### Using this Library

To parse a todo.txt file on your local machine:

```
var todoList = TodoTxtCollection.FromFile(@"C:\Users\username\Desktop\todo.txt");
```

To create a new to-do item and write it to a file:

```
var newTodo = new TodoTxt();
newTodo.Description = "Thank Mom for the meatballs @phone";
newTodo.Priority = 'A';
var todoList = new TodoTxtCollection(newTodo);
todoList.SaveToFile(@"C:\Users\username\Desktop\newTodo.txt")
```

The `newTodo.txt` file now contains this line:

```
(A) Thank Mom for the meatballs @phone
```

