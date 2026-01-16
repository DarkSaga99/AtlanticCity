$path = "$PSScriptRoot\Carga_Pesada_6MB.csv"
$sw = [System.IO.StreamWriter]::new($path)
$sw.WriteLine('Codigo,Nombre,Periodo')
for ($i = 1; $i -le 110000; $i++) {
    $line = "PROD-{0:d6},Producto de Prueba Generado de Largo Alcance - Fila {1},2026-01" -f $i, $i
    $sw.WriteLine($line)
}
$sw.Close()
Write-Host "Archivo generado en: $path"
