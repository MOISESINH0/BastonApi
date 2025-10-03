namespace Baston.Domain.Entities
{
    public class ConfianzaRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SenderId { get; set; }   // Usuario que envía la solicitud
        public AppUser Sender { get; set; }

        public Guid ReceiverId { get; set; } // Usuario que recibe la solicitud
        public AppUser Receiver { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Estados posibles: Pending, Accepted, Rejected
        public string Status { get; set; } = "Pending";
    }
}
