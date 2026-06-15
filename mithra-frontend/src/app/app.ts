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

  public filtroActual = signal<string>('Todos');
  public busquedaTexto = signal<string>(''); 

  public productosFiltrados = computed(() => {
    let articulos = this.productosService.listaProductos();
    const filtro = this.filtroActual();
    const texto = this.busquedaTexto().toLowerCase().trim();

    if (filtro !== 'Todos') {
      articulos = articulos.filter(p => p.tipo === filtro);
    }

    if (texto !== '') {
      articulos = articulos.filter(p => 
        p.codigo.toLowerCase().includes(texto) || 
        p.nombre.toLowerCase().includes(texto)
      );
    }
    return articulos;
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
  }

  public actualizarBusqueda(evento: Event): void {
    const valor = (evento.target as HTMLInputElement).value;
    this.busquedaTexto.set(valor);
  }
  public async eliminarProducto(id: number): Promise<void> {
    if (confirm('¿Estás seguro de que deseas eliminar este producto?')) {
      try {
        await this.productosService.eliminarProducto(id);
        
        // Actualizamos la señal para que la UI se refresque sola
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