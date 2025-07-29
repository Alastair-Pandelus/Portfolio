namespace Portfolio.EntityModel
{
    // A proxy is used when insufficient pricing data is available for an instrument.
    // The proxy instrument is used to calculate the value of the instrument.
    // The can be multiple weighted proxies, leverage is possible for 'stacked' instruments.
    public class ProxyInstrument
    {
        public int Id { get; set; }
        public int InstrumentId { get; set; }
        public Instrument Instrument { get; set; }
        public int ProxyId { get; set; }
        public Instrument Proxy { get; set; }
        public double Weight { get; set; }
    }
}
