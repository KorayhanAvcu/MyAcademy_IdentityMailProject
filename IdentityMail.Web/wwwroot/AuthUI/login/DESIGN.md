---
name: Lüminans Tasarım Sistemi
colors:
  surface: '#faf9fd'
  surface-dim: '#dbd9dd'
  surface-bright: '#faf9fd'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f4f3f7'
  surface-container: '#efedf1'
  surface-container-high: '#e9e7eb'
  surface-container-highest: '#e3e2e6'
  on-surface: '#1a1b1e'
  on-surface-variant: '#414754'
  inverse-surface: '#2f3033'
  inverse-on-surface: '#f1f0f4'
  outline: '#727785'
  outline-variant: '#c1c6d6'
  surface-tint: '#005bc0'
  primary: '#005bbf'
  on-primary: '#ffffff'
  primary-container: '#1a73e8'
  on-primary-container: '#ffffff'
  inverse-primary: '#adc7ff'
  secondary: '#005ac1'
  on-secondary: '#ffffff'
  secondary-container: '#4d8efe'
  on-secondary-container: '#00285c'
  tertiary: '#565e6a'
  on-tertiary: '#ffffff'
  tertiary-container: '#6f7783'
  on-tertiary-container: '#ffffff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d8e2ff'
  primary-fixed-dim: '#adc7ff'
  on-primary-fixed: '#001a41'
  on-primary-fixed-variant: '#004493'
  secondary-fixed: '#d8e2ff'
  secondary-fixed-dim: '#adc6ff'
  on-secondary-fixed: '#001a41'
  on-secondary-fixed-variant: '#004494'
  tertiary-fixed: '#dbe3f1'
  tertiary-fixed-dim: '#bfc7d4'
  on-tertiary-fixed: '#141c26'
  on-tertiary-fixed-variant: '#3f4752'
  background: '#faf9fd'
  on-background: '#1a1b1e'
  surface-variant: '#e3e2e6'
typography:
  display-lg:
    fontFamily: Inter
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 56px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
    letterSpacing: -0.01em
  headline-lg-mobile:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  title-md:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '500'
    lineHeight: 24px
  body-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  label-md:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
    letterSpacing: 0.05em
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  base: 4px
  xs: 8px
  sm: 12px
  md: 16px
  lg: 24px
  xl: 32px
  container-max: 1440px
  gutter: 16px
  margin-mobile: 16px
  margin-desktop: 24px
---

## Marka ve Stil
Bu tasarım sistemi, modern e-posta iletişimi için profesyonellik, hız ve güven üzerine inşa edilmiştir. Kullanıcıyı yormayan, odaklanmayı kolaylaştıran **Minimalist** bir yaklaşım benimser.

**Hedef Kitle:** Yoğun iş temposunda çalışan profesyoneller, kurumsal ekipler ve düzenli bir dijital alan arayan bireysel kullanıcılar.

**Duygusal Tepki:**
- **Güven:** Kararlı ve temiz hatlarla sağlanan kurumsal ciddiyet.
- **Berraklık:** Karmaşadan uzak, geniş boşluklu (whitespace) yerleşim ile gelen zihinsel rahatlık.
- **Verimlilik:** Hızlı tarama ve eyleme geçme imkanı tanıyan hiyerarşik düzen.

Tasarım dili, Google'ın Material Design ilkelerinden esinlenen ancak daha keskin bir tipografik hiyerarşi ve daha yumuşak gölge geçişleriyle ayrışan modern bir kurumsal estetiğe sahiptir.

## Renkler
Renk paleti, okunabilirliği en üst düzeye çıkarmak ve kullanıcıyı eyleme yönlendirmek için stratejik olarak yapılandırılmıştır.

- **Birincil (Primary):** `#1a73e8` rengi, butonlar, aktif durumlar ve kritik bağlantılar için ana odak noktasıdır.
- **Yüzeyler:** Arka plan için `#f8f9fa` kullanılarak içerik alanları ile uygulama çerçevesi arasında hafif bir ayrım oluşturulur. Ana içerik kartları ve e-posta listeleri saf beyaz (`#ffffff`) yüzeylerde sunulur.
- **Metin:** Gövde metni ve başlıklar için yüksek kontrastlı `#202124` tercih edilerek göz yorgunluğu en aza indirilir.
- **Vurgu:** İkincil etkileşimler ve seçili öğe arka planları için yumuşak bir mavi olan `#e8f0fe` (Tertiary) kullanılır.

## Tipografi
Sistem, yüksek okunabilirlik ve modern bir duruş sergileyen **Inter** font ailesini temel alır.

- **Hiyerarşi:** Başlıklarda daha kalın font ağırlıkları (600-700) tercih edilirken, uzun okuma metinlerinde (e-posta gövdesi) optik denge için 14px ve 16px boyutları kullanılır.
- **Okunabilirlik:** Satır yükseklikleri (line-height), metin bloklarının nefes almasını sağlayacak şekilde geniş tutulmuştur.
- **Etiketler:** Sistem ikonları ve küçük yardımcı metinler için `label-md` seviyesi, tamamen büyük harf veya belirgin bir ağırlıkla sunulmalıdır.
- **Mobil Adaptasyon:** Büyük başlıklar mobil cihazlarda otomatik olarak bir alt ölçeğe (`headline-lg-mobile`) indirgenir.

## Yerleşim ve Boşluklar
Bu tasarım sistemi, 4px bazlı bir ızgara sistemi (8-pt grid sisteminin alt kümesi) üzerine kuruludur.

- **Izgara Yapısı:** Masaüstünde 12 sütunlu, esnek bir ızgara kullanılır. Kenar çubukları (Sidebar) sabit genişlikte (256px) kalırken, ana içerik alanı akışkandır.
- **Hava Payı:** İçerik kartları arasında `lg` (24px) boşluk bırakılarak öğelerin birbirine karışması önlenir.
- **Duyarlı Tasarım (Responsive):**
  - **Mobil:** Tek sütun düzeni, 16px yan marjlar. Alt navigasyon çubuğu kullanımı.
  - **Tablet:** Daraltılabilir sol menü, ızgara sütunları 8'e düşer.
  - **Masaüstü:** Tam açık navigasyon, 12 sütunlu düzen.

## Yükseklik ve Derinlik
Derinlik algısı, geleneksel ağır gölgeler yerine **tonal katmanlar** ve son derece yumuşak, geniş dağılımlı ambiyans gölgeleri ile sağlanır.

- **Katman 0 (Zemin):** `#f8f9fa` rengindeki ana arka plan.
- **Katman 1 (İçerik):** Beyaz yüzeyli kartlar. Gölge yok, sadece 1px değerinde `#e0e0e0` çerçeve (outline).
- **Katman 2 (Etkileşim):** Üzerine gelinen (hover) öğeler veya açılır menüler. Hafif bir gölge kullanılır: `0px 4px 12px rgba(0, 0, 0, 0.05)`.
- **Katman 3 (Modal/Floating):** En üst seviyedeki diyalog pencereleri. Belirgin ama temiz gölge: `0px 8px 24px rgba(0, 0, 0, 0.12)`.

## Şekiller
Şekil dili, profesyonelliği korurken kullanıcıya dostça bir deneyim sunmak için **"Rounded" (Yuvarlatılmış)** olarak belirlenmiştir.

- **Standart Bileşenler:** Butonlar, giriş alanları ve kartlar 8px (`0.5rem`) köşe yarıçapına sahiptir.
- **Vurgulu Öğeler:** "E-posta Oluştur" (Compose) gibi eylem butonlarında `rounded-xl` (24px) kullanılarak daha davetkar ve ayrışan bir görünüm elde edilir.
- **Seçim Göstergeleri:** Liste üzerindeki aktif öğe işaretçileri genellikle sol tarafı yuvarlatılmış hap formunda tasarlanır.

## Bileşenler
Tüm bileşenler, tutarlılık ve erişilebilirlik standartlarına uygun olarak tasarlanmıştır.

- **Butonlar:**
    - *Primary:* Dolgulu mavi arka plan, beyaz metin. Tam yuvarlatılmış veya 8px köşe.
    - *Secondary:* Mavi ana hatlı (outline) veya şeffaf arka planlı metin butonları.
- **Giriş Alanları (Inputs):**
    - Odaklanıldığında (focus) birincil mavi renkte 2px kalınlığında çerçeve belirir.
    - Etiketler, yazım başladığında yukarı kayan (floating label) veya alanın üzerinde net bir şekilde konumlanan `label-md` stilindedir.
- **Kartlar:**
    - E-posta listelerindeki her bir öğe, hover durumunda arka planı `#f1f3f4` rengine dönen, hafif ayırıcılı satırlar şeklindedir.
- **Çipler (Chips):**
    - Etiketler (Labels) için kullanılır. Dolgu rengi açık gri veya pastel tonlarda, metin rengi ise aynı tonun koyusu olacak şekilde tasarlanır.
- **Listeler:**
    - Okunmamış e-postalar için font ağırlığı `600`, okunmuşlar için `400` olarak belirlenmiştir.