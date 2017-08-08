#Pipes on C#

```csharp
var result = Pipeline
  .Start(() => 10, x => x + 6)
  .Pipe(x => x.ToString())
  .Pipe(int.Parse)
  .Pipe(x => Math.Sqrt(x))
  .Pipe(x => x*5)
  .Execute();
```
