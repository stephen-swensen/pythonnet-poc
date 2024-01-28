using System.Runtime.Serialization;
using Python.Runtime;

namespace pythonnet_poc;

class Program
{
/**
 * According to https://opensource.com/article/17/4/grok-gil
 * "IronPython provide single-process parallelism, but they are far from full CPython compatibility (e.g. no numpy)"
 *
 * Install python in dotnet base container: https://stackoverflow.com/questions/68607222/how-to-run-a-python-script-inside-a-net-core-application-in-a-docker-container
 */
  static void Main(string[] args)
  {
    Console.WriteLine("Hello World");
    //find libpython3.x.so on linux:
    //sudo find / -name libpython3.10.so
    var libpythonPath = "/usr/lib/python3.10/config-3.10-x86_64-linux-gnu/libpython3.10.so";
    //alternatively set PYTHONNET_PYDLL env var
    Runtime.PythonDLL = libpythonPath;
    PythonEngine.Initialize(); //initialize once from main thread
    //https://github.com/pythonnet/pythonnet/issues/109#issuecomment-184143936
    //"The main thread will hold the GIL after initialization until you explicitly release it by calling PythonEngine.BeginAllowThreads() from the main thread (not from your background thread). This is how python threading works, it's not specific to pythonnet."
    PythonEngine.BeginAllowThreads();


    string code = @"fullName === person.FirstName + ' ' + person.LastName + ' ' + str(np.sin(5))
  fullName = person.FirstName + ' ' ++ person.LastName + ' ' + str(np.sin(5))";
    PyObject compiledCode;
    using(var _ = Py.GIL()) //yes, the GIL is needed here
    {
      try
      {
        compiledCode = PythonEngine.Compile(code);

      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message.Replace("(, line", "(line"));
        return;
      }
    }

    Console.WriteLine("made it");

    using(var _ = Py.GIL())
    {
      var person = new { FirstName = "John", LastName = "Smith" };

      // create a Python scope
      using (PyModule scope = Py.CreateScope())
      {
        scope.Import("numpy", "np");
        // convert the Person object to a PyObject
        PyObject pyPerson = person.ToPython();

        // create a Python variable "person"
        scope.Set("person", pyPerson);

        scope.Execute(compiledCode);

        Console.WriteLine(scope.Get("fullName"));
      }
    }

    //never returns
    //PythonEngine.EndAllowThreads(threadState);

    //slowness concerns with Python.Shutdown... is it really needed?
    //https://github.com/pythonnet/pythonnet/issues/2008
    //PythonEngine.Shutdown();
  }
}
