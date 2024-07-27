using System;
using System.IO;
using System.Text;

public class MultiTextWriter : TextWriter
{
    private readonly TextWriter consoleWriter;
    private readonly TextWriter fileWriter;

    public MultiTextWriter(TextWriter consoleWriter, TextWriter fileWriter)
    {
        this.consoleWriter = consoleWriter;
        this.fileWriter = fileWriter;
    }

    public override Encoding Encoding => consoleWriter.Encoding;

    public override void Write(char value)
    {
        consoleWriter.Write(value);
        fileWriter.Write(value);
    }

    public override void Write(string value)
    {
        consoleWriter.Write(value);
        fileWriter.Write(value);
    }

    public override void WriteLine(string value)
    {
        consoleWriter.WriteLine(value);
        fileWriter.WriteLine(value);
    }
}
