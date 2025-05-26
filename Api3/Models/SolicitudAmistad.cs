namespace api3.Models
{
    public class SolicitudAmistad
    {
        public int Id { get; set; }
        public string RemitenteEmail { get; set; }
         public string ReceptorEmail { get; set; }

        public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;
         public DateTime FechaEnvio { get; set; }=DateTime.UtcNow;


    }
    public enum EstadoSolicitud 
    {
        Pendiente,
        Aceptada,
        Rechazada,
        Cancelada
    
    
    }
}
