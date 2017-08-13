历史(HST)文件格式
http://blog.sina.com.cn/s/blog_dd41b3150101dk2m.html
http://blog.csdn.net/u012285618/article/details/54847146
https://github.com/hamling-ling/FxResearches/tree/master/hstconverter

2013.4.15更新：
到了本文的关键之处，如果我们想直接读取mt4的hst历史文件来获取数据，就必须先搞清楚该二进制文件的数据格式。
最近编程需要，涉及用C++或PHP来读取mt4的历史文件。所以，特意用Binary Viewer打开该类型的文件，做了一些分析：
0-147字节，为文件头：
struct Header
{
00    int           version;               // 版本号
04    char        copyright[64];     // 版权信息
68    char        symbol[12];         // 货币对名称，如"EURUSD"
80    int           period;                // 数据周期：15代表 M15周期
84    int           digits;                 // 数据格式：小数点位数     //例如5，代表有效值至小数点5位，1.
88    time_t     time sign;           // 文件的创建时间
...     ...
...     ...
};
从148字节开始，是数据排列，每项数据为44字节，结构如下:
第一项数据
struct RateInfo
{
148    time_t       ctm;                 // 以秒计算当前时间
152    double      open;
160    double      low;
168    double      high;
176    double      close;
184    double      vol;
};
第二项数据：
192    time_t       ctm; 
196    double      open;
...       ......
...       ......
注：mt4终端关闭后，针对mt4终端已打开的图表，历史数据会保存于相应的hst文件中。