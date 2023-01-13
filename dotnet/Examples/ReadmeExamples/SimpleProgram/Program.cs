using LysitheaVM;

public static class Program
{
    #region Methods
    public static void Main()
    {
        var assembler = new Assembler();
        StandardLibrary.AddToScope(assembler.BuiltinScope);

        var script = assembler.ParseFromText("ReadmeExample", "(print 'Result ' (+ 5 12))");

        var vm = new VirtualMachine(8);
        vm.Execute(script);
    }
    #endregion
}