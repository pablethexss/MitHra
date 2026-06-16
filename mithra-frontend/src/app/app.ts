import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { ProductosService } from './services/producto'; 
import { ProductosComponent } from './components/productos/productos';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, ProductosComponent],
  templateUrl: './app.html', 
  styleUrl: './app.css'
})
export class AppComponent implements OnInit {
  public productosService = inject(ProductosService);
  
  public temaActual: string = 'industrial';

  // --- Signals para filtros y búsqueda ---
  public filtroActual = signal<string>('Todos');
  public busquedaTexto = signal<string>(''); 

  // --- Paginación ---
  public currentPage = signal<number>(1);
  public itemsPerPage = 10;

  // --- Lógica Computada: Filtrado Original ---
  public productosFiltradosOriginales = computed(() => {
    let articulos = this.productosService.listaProductos();
    const filtro = this.filtroActual();
    const texto = this.busquedaTexto().toLowerCase().trim();
    
    // 1. Filtrado por categoría o estado especial
    if (filtro === 'Reponer') {
        // Filtra solo Físicos que necesiten reposición (Agotados o críticos)
        articulos = articulos.filter(p => 
            p.tipo !== 'Servicio' && (p.stock === 0 || p.stock <= p.stockMinimo)
        );
    } else if (filtro !== 'Todos') {
        // Filtra por tipo normal (Fisico o Servicio)
        articulos = articulos.filter(p => p.tipo === filtro);
    }

    // 2. Filtrado por texto (búsqueda)
    if (texto !== '') {
        articulos = articulos.filter(p => 
            p.codigo.toLowerCase().includes(texto) || 
            p.nombre.toLowerCase().includes(texto)
        );
    }
    
    return articulos;
});
  // --- Lógica Computada: Paginación ---
  public productosPaginados = computed(() => {
    const articulos = this.productosFiltradosOriginales();
    const pagina = this.currentPage();
    const startIndex = (pagina - 1) * this.itemsPerPage;
    return articulos.slice(startIndex, startIndex + this.itemsPerPage);
  });

  public totalPaginas = computed(() => {
    const total = this.productosFiltradosOriginales().length;
    return Math.max(1, Math.ceil(total / this.itemsPerPage));
  });

  ngOnInit(): void {
    this.productosService.cargarProductos();
    this.cambiarTema('industrial');
  }

  public cambiarTema(nuevoTema: string): void {
    this.temaActual = nuevoTema;
    document.body.setAttribute('data-theme', nuevoTema);
  }

  public aplicarFiltro(nuevoFiltro: string): void {
    this.filtroActual.set(nuevoFiltro);
    this.currentPage.set(1); // Resetear a página 1 al filtrar
  }

  public actualizarBusqueda(evento: Event): void {
    const valor = (evento.target as HTMLInputElement).value;
    this.busquedaTexto.set(valor);
    this.currentPage.set(1); // Resetear a página 1 al buscar
  }

  public cambiarPagina(n: number): void {
    this.currentPage.set(n);
  }

  public async eliminarProducto(id: number): Promise<void> {
    if (confirm('¿Estás seguro de que deseas eliminar este producto?')) {
      try {
        await this.productosService.eliminarProducto(id);
        this.productosService.listaProductos.update(lista => 
          lista.filter(p => p.id !== id)
        );
      } catch (error) {
        console.error('Error al eliminar:', error);
        alert('No se pudo eliminar el producto.');
      }
    }
  }   
}