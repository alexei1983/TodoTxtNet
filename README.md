## TodoTxtNet
A library for reading and writing todo.txt files in .NET

### Introduction

The todo.txt format is an informally defined standard for representing tasks and other to-do items in a simple 
plaintext file.

The ubiquity of plaintext means that todo.txt files are inherently cross-platform and can be synced across multiple devices.

This library provides tools to read and write todo.txt files in .NET 8.

Check out the [todo.txt page on Github](https://github.com/todotxt/todo.txt)

### Using this Library

To parse a todo.txt file on your local machine:

```
var todoList = TodoTxtList.FromFile(@"C:\Users\username\Desktop\todo.txt");
```

To create a new to-do item and write it to a file:

```
var newTodo = new TodoTxt();
newTodo.Description = "Thank Mom for the meatballs @phone";
newTodo.Priority = 'A';
var newTodoList = new TodoTxtList(newTodo);
newTodoList.SaveToFile(@"C:\Users\username\Desktop\newTodo.txt")
```

The `newTodo.txt` file now contains this line:

```
(A) Thank Mom for the meatballs @phone
```

To filter the `todoList` parsed above, use the various extension methods:

```
var superImportantTodos = todoList.Priorities('A', 'B')
                                  .ForContext("Work")
                                  .ForProject("NewClient")
                                  .Incomplete();
```

The variable `superImportantTodos` now contains the filtered and sorted to-do list.
