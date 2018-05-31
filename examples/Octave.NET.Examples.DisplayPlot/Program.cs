namespace Octave.NET.Examples.DisplayPlot
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var octave = new OctaveContext())
            {
                // Note: your octave-cli must support some plot backend, in case of problems investigate manually in octave-cli

                var script = @"
x = -10:0.1:10;
y = sin (x);

handle = plot (x, y);

title (""Hello from C#"");
xlabel (""x"");
ylabel (""sin (x)"");

waitfor(handle); #                 <- without that plot window would not show 
                ";

                octave.Execute(script, int.MaxValue);
            }
        }
    }
}
