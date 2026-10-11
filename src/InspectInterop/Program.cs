using Mono.Cecil;
var assembly=AssemblyDefinition.ReadAssembly(args[0]);
var names=new HashSet<string>(args.Skip(1));
foreach(var type in assembly.MainModule.Types.Where(t=>names.Contains(t.Name)))
{
    Console.WriteLine("TYPE "+type.FullName+" BASE "+type.BaseType?.FullName);
    foreach(var field in type.Fields.Where(f=>!f.Name.StartsWith("Native"))) Console.WriteLine("FIELD "+field.FieldType.FullName+" "+field.Name);
    foreach(var property in type.Properties) Console.WriteLine("PROPERTY "+property.PropertyType.FullName+" "+property.Name);
    foreach(var method in type.Methods.Where(m=>!m.Name.StartsWith("get_")&&!m.Name.StartsWith("set_")&&!m.IsConstructor)) Console.WriteLine("METHOD "+method.ReturnType.FullName+" "+method.Name+"("+string.Join(", ",method.Parameters.Select(p=>p.ParameterType.FullName+" "+p.Name))+")");
}
