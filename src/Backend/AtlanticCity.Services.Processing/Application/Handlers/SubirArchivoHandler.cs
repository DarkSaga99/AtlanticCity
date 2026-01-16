using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AtlanticCity.Servicios.Procesamiento.Core.Interfaces;
using AtlanticCity.Compartido.Mensajeria;
using AtlanticCity.Compartido.Mensajeria.Dtos;
using AtlanticCity.Services.Processing.Application.Commands;

namespace AtlanticCity.Services.Processing.Application.Handlers
{
  
    public class SubirArchivoHandler : IRequestHandler<SubirArchivoCommand, Guid>
    {
        // DEPENDENCIAS: Todas son INTERFACES (abstracciones), no clases concretas
        private readonly IRepositorioLote _repositorioLote;        // Para gestión del lote
        private readonly IAuditoriaRepositorio _auditoria;         // Para registro de auditoría (SOLID)
        private readonly IServicioAlmacenamiento _almacenamiento;  // Para SeaweedFS
        private readonly IServicioMensajeria _mensajeria;          // Para RabbitMQ

        // CONSTRUCTOR: Inyección de Dependencias (DI)
        public SubirArchivoHandler(
            IRepositorioLote repositorioLote,
            IAuditoriaRepositorio auditoria,
            IServicioAlmacenamiento almacenamiento, 
            IServicioMensajeria mensajeria)
        {
            _repositorioLote = repositorioLote;
            _auditoria = auditoria;
            _almacenamiento = almacenamiento;
            _mensajeria = mensajeria;
        }

        // MÉTODO HANDLE: Orquesta el flujo de negocio
        public async Task<Guid> Handle(SubirArchivoCommand request, CancellationToken cancellationToken)
        {
            // 0. VALIDACIONES DE NEGOCIO (Domain Logic)
            if (request.Archivo == null || request.Archivo.Length == 0)
                throw new ArgumentException("El archivo es requerido.");

            var extension = System.IO.Path.GetExtension(request.Archivo.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || (extension != ".csv" && extension != ".xlsx"))
                throw new ArgumentException("Formato no permitido. Solo se aceptan .csv y .xlsx");

            // 1. GENERAR ID ÚNICO para trazabilidad del lote
            var batchId = Guid.NewGuid();

            // 2. PERSISTENCIA: Delegamos al repositorio (el Handler no sabe que es SQL Server)
            await _repositorioLote.CrearLoteAsync(batchId, request.Archivo.FileName, request.CorreoUsuario);

            // 3. ALMACENAMIENTO: Subimos el archivo físico a SeaweedFS
            using var stream = request.Archivo.OpenReadStream();
            var url = await _almacenamiento.SubirArchivoAsync(stream, request.Archivo.FileName);

            // 4. MENSAJERÍA: Publicamos en RabbitMQ para que el Worker lo procese
            var mensaje = new MensajeProcesamientoArchivo
            {
                IdMensaje = batchId,
                NombreArchivo = request.Archivo.FileName,
                UrlArchivo = url,
                CorreoUsuario = request.CorreoUsuario
            };
            await _mensajeria.PublicarMensajeAsync("cola_procesamiento_archivos", mensaje);

            // 5. AUDITORÍA: Registramos la acción (Delegada a interfaz específica)
            try
            {
                await _auditoria.RegistrarAsync(
                    "Inicio de Carga", 
                    "ProcessBatches", 
                    request.CorreoUsuario, 
                    $"Se ha iniciado la subida del archivo '{request.Archivo.FileName}' exitosamente."
                );
            }
            catch (Exception ex)
            {
                await _auditoria.RegistrarAsync(
                    "Error de Carga", 
                    "ProcessBatches", 
                    request.CorreoUsuario, 
                    $"Fallo al procesar subida de '{request.Archivo.FileName}': {ex.Message}"
                );
            }

            // 6. RETORNO: Devolvemos el ID para que el frontend pueda rastrear el lote
            return batchId;
        }
    }
}
