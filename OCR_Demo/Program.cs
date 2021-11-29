using System;
using System.Drawing;
using Tesseract;

namespace OCR_Demo
{
    class Program
    {
        static void Main(string[] args)
        {            
            var diretorioImagem = @"C:\Users\Cheron\Downloads\OCR_Testes\Arquivos\rg-real.jpg";

            try
            {
                using (var engrenagemOCR = new TesseractEngine(@"tessdata", "por", EngineMode.Default))
                {
                    using (var imagemBruta = Pix.LoadFromFile(diretorioImagem))
                    {
                        Bitmap processoFiltro;
                        Bitmap bmp = PixConverter.ToBitmap(imagemBruta);
                        
                        processoFiltro = Redimensionar(bmp, 1920, 1080);
                        processoFiltro = RemoverRuido(processoFiltro);                                                
                        Pix imagemFiltrada = PixConverter.ToPix(processoFiltro);
                       
                        using (var pagina = engrenagemOCR.Process(imagemFiltrada))
                        {
                            var textoExtraido = pagina.GetUNLVText();
                            Console.WriteLine("Prescision: {0}", pagina.GetMeanConfidence());
                            Console.WriteLine("Text: \r\n{0}", textoExtraido);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.Message.ToString());
            }
            finally
            {
                Console.ReadKey();
            }
        }
                
        //Redimensione a imagem com altura e largura variáveis ​​(multiplique 0,5, 1 e 2 pela altura e largura da imagem).
        public static Bitmap Redimensionar(Bitmap bmp, int novaLargura, int novaAltura)
        {
            // Construtor armazena em memória altura, largura e os pixels de redimensionamento
            Bitmap temp = (Bitmap)bmp;
            Bitmap bmap = new Bitmap(novaLargura, novaAltura, temp.PixelFormat);

            // Variáveis para Calculo de Fatores
            double novoFatorLargura = (double)temp.Width / (double)novaLargura;
            double novoFatorAltura = (double)temp.Height / (double)novaAltura;

            // Variaveis e objetos para o uso do filtro de cores no redimensionamento da imagem
            double fx, fy, nx, ny;
            int cx, cy, fr_x, fr_y;
            Color color1 = new Color();
            Color color2 = new Color();
            Color color3 = new Color();
            Color color4 = new Color();
            byte nRed, nGreen, nBlue;
            byte bp1, bp2;

            for (int x = 0; x < bmap.Width; ++x)
            {
                for (int y = 0; y < bmap.Height; ++y)
                {

                    // Recalculo de redimencionamento. Utilzando as variáveis de fatores
                    fr_x = (int)Math.Floor(x * novoFatorLargura);
                    fr_y = (int)Math.Floor(y * novoFatorAltura);
                    cx = fr_x + 1;
                    if (cx >= temp.Width) cx = fr_x;
                    cy = fr_y + 1;
                    if (cy >= temp.Height) cy = fr_y;
                    fx = x * novoFatorLargura - fr_x;
                    fy = y * novoFatorAltura - fr_y;
                    nx = 1.0 - fx;
                    ny = 1.0 - fy;

                    // Captura de pixels para redimensionamento
                    color1 = temp.GetPixel(fr_x, fr_y);
                    color2 = temp.GetPixel(cx, fr_y);
                    color3 = temp.GetPixel(fr_x, cy);
                    color4 = temp.GetPixel(cx, cy);

                    // Esquema de cores em RGB
                    // setando cores de redimensionamento: Azul
                    bp1 = (byte)(nx * color1.B + fx * color2.B);
                    bp2 = (byte)(nx * color3.B + fx * color4.B);
                    nBlue = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    // setando cores de redimensionamento: Verde
                    bp1 = (byte)(nx * color1.G + fx * color2.G);
                    bp2 = (byte)(nx * color3.G + fx * color4.G);
                    nGreen = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    // setando cores de redimensionamento: Vermelho
                    bp1 = (byte)(nx * color1.R + fx * color2.R);
                    bp2 = (byte)(nx * color3.R + fx * color4.R);
                    nRed = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    // Construção de cores na imagem redimencionada em RGB
                    bmap.SetPixel(x, y, System.Drawing.Color.FromArgb(245, nRed, nGreen, nBlue));
                }
            }

            return bmap;
        }

        // Definir escala de cinza para facilitar a leitura de caracteres da tecnologia
        public static Bitmap RemoverRuido(Bitmap imagemBruta)
        {
            Bitmap temp = (Bitmap)imagemBruta;
            Bitmap bmap = (Bitmap)temp.Clone();
            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    byte gray = (byte)(.299 * c.R + .587 * c.G + .114 * c.B);

                    bmap.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }
            return (Bitmap)bmap.Clone();
        }              
    }
}