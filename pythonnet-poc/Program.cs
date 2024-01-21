using Python.Runtime;

namespace pythonnet_poc;

class Program
{
  static void Main(string[] args)
  {
    try
    {
      Console.WriteLine("Hello World");
      //find libpython3.x.so on linux:
      //sudo find / -name libpython3.10.so
      var libpythonPath = "/usr/lib/python3.10/config-3.10-x86_64-linux-gnu/libpython3.10.so";
      Runtime.PythonDLL = libpythonPath;
      PythonEngine.Initialize();
      PythonEngine.BeginAllowThreads();

      using var _ = Py.GIL();

      //install pip first if not already installed:
      //sudo apt install python3-pip
      //then install numpy
      //pip install numpy
      dynamic np = Py.Import("numpy");
      Console.WriteLine(np.cos(np.pi * 2));

      dynamic sin = np.sin;
      Console.WriteLine(sin(5));

      double c = (double)(np.cos(5) + sin(5));
      Console.WriteLine(c);

      dynamic a = np.array(new List<float> { 1, 2, 3 });
      Console.WriteLine(a.dtype);

      dynamic b = np.array(new List<float> { 6, 5, 4 }, dtype: np.int32);
      Console.WriteLine(b.dtype);

      Console.WriteLine(a * b);
      Console.ReadKey();
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
  }
}
