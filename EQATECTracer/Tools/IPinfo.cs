using System.Xml.Serialization;

namespace EQATEC.Tracer.Tools
{
  /// <summary>
  /// Holds "ip, port, name" for a connection.
  /// </summary>
  public class IPinfo
  {
    [XmlAttribute("ip")]
    public string ip;
    [XmlAttribute("port")]
    public int port;
    [XmlAttribute("name")]
    public string name;

    public IPinfo() { }
    public IPinfo(string name, string ip, int port)
    {
      this.ip = ip;
      this.port = port;
      this.name = name;
    }
    public override string ToString()
    {
      //return (name+":"+ip+":"+port); //When tracer supports naming and different ports
      return (ip);
    }
  }
}