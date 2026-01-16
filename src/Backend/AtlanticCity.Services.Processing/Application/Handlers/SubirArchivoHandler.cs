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
        private readonly IRepositorioLote _repositorioLote;
        private readonly IAuditoriaRepositorio _auditoria;
        private readonly IServicioAlmacenamiento _almacenamiento;
        private readonly IServicioMensajeria _mensajeria;

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

        public async Task<Guid> Handle(SubirArchivoCommand request, CancellationToken cancellationToken)
        {
            if (request.Archivo == null || request.Archivo.Length == 0)
                throw new ArgumentException("File is required");

            var extension = System.IO.Path.GetExtension(request.Archivo.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || (extension != ".csv" && extension != ".xlsx"))
                throw new ArgumentException("Invalid format. Only .csv and .xlsx are allowed");

            var batchId = Guid.NewGuid();

            // Store metadata
            await _repositorioLote.CrearLoteAsync(batchId, request.Archivo.FileName, request.CorreoUsuario);

            // Upload to storage
            using var stream = request.Archivo.OpenReadStream();
            var url = await _almacenamiento.SubirArchivoAsync(stream, request.Archivo.FileName);

            // Send to processing queue
            var mensaje = new MensajeProcesamientoArchivo
            {
                IdMensaje = batchId,
                NombreArchivo = request.Archivo.FileName,
                UrlArchivo = url,
                CorreoUsuario = request.CorreoUsuario
            };
            await _mensajeria.PublicarMensajeAsync("cola_procesamiento_archivos", mensaje);

            // Audit the operation
            try
            {
                await _auditoria.RegistrarAsync(
                    "Upload Started", 
                    "ProcessBatches", 
                    request.CorreoUsuario, 
                    $"Started processing '{request.Archivo.FileName}'"
                );
            }
            catch (Exception)
            {
                // Soft fail for audit logs
            }

            return batchId;
        }
    }
}
