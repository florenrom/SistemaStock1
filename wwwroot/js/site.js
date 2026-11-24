$(document).ready(function () {

    // Inicializar todas las tablas del sistema
    $('.sf-table').DataTable({
        language: {
            url: 'https://cdn.datatables.net/plug-ins/2.3.2/i18n/es-ES.json'
        }
    });

    //// Tabla de Categorías
    //$('#tablaCategorias').DataTable({
    //    language: {
    //        url: 'https://cdn.datatables.net/plug-ins/2.3.2/i18n/es-ES.json'
    //    }
    //});

    //// Tabla de Productos
    //$('#tablaProductos').DataTable({
    //    language: {
    //        url: 'https://cdn.datatables.net/plug-ins/2.3.2/i18n/es-ES.json'
    //    }
    //});

    //// Tabla de Movimientos
    //$('#tablaMovimientos').DataTable({
    //    language: {
    //        url: 'https://cdn.datatables.net/plug-ins/2.3.2/i18n/es-ES.json'
    //    }
    //});

});
// Confirmación de eliminación con SweetAlert2
document.querySelectorAll('.form-eliminar').forEach(form => {

    form.addEventListener('submit', function (e) {

        e.preventDefault();

        const nombre = this.dataset.nombre || 'este registro';

        Swal.fire({
            title: '¿Estás seguro?',
            text: `¿Deseás eliminar ${nombre}?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {

            if (result.isConfirmed) {
                this.submit();
            }

        });

    });

});