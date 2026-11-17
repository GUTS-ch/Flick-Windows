using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Windows.Input;
using System.IO;

namespace FlickWindows;
public partial class MainWindow : Window {
 const int HotkeyId=0x51; const uint MOD_CONTROL=2, MOD_SHIFT=4; static readonly HttpClient Http=new(); string selected="";
 readonly (string Icon,string Name,string Prompt)[] presets={("📖","解释","请清晰简洁地解释以下内容。如果是词语或短语，请给出定义和用法。"),("📝","总结","请简洁地总结以下内容，提炼关键要点。"),("🌐","翻译","请将以下内容翻译为中文。只输出翻译结果，不需要解释。"),("✏️","润色","请润色和改进以下内容，保持原意不变，使其更流畅、更专业。"),("💡","续写","请根据以下内容继续写作，保持一致的风格和语气。")};
 public MainWindow(){InitializeComponent(); foreach(var p in presets){var b=new Button{Content=$"{p.Icon} {p.Name}"}; b.Click+=async(_,_)=>await Ask(p.Prompt); Prompts.Children.Add(b);} Loaded+=(_,_)=>RegisterHotkey(); Closed+=(_,_)=>UnregisterHotKey(new WindowInteropHelper(this).Handle,HotkeyId);}
 void RegisterHotkey(){var h=new WindowInteropHelper(this).Handle; var ok=RegisterHotKey(h,HotkeyId,MOD_CONTROL|MOD_SHIFT,0x45); Status.Text=ok?"已就绪：Ctrl+Shift+E":"快捷键注册失败（可能被其他程序占用）"; HwndSource.FromHwnd(h)?.AddHook((IntPtr hwnd,int msg,IntPtr w,IntPtr l,ref bool handled)=>{if(msg==0x0312){handled=true;Dispatcher.BeginInvoke(CaptureSelection);}return IntPtr.Zero;});}
 async void CaptureSelection(){Hide();await Task.Delay(180);var data=DataObjectCopy();selected=data;if(string.IsNullOrWhiteSpace(selected)){Show();Activate();Status.Text="未读取到选中文字，请先选中内容再按 Ctrl+Shift+E";return;}Output.Text=selected;Status.Text="已读取选中文字，请选择操作";Show();Activate();}
 static string DataObjectCopy(){try{Clipboard.Clear();}catch{} Thread.Sleep(100);SendKeysCopy();Thread.Sleep(450);try{return Clipboard.ContainsText()?Clipboard.GetText():"";}catch{return "";}}
 static void SendKeysCopy(){keybd_event(0x11,0,0,UIntPtr.Zero);Thread.Sleep(40);keybd_event(0x43,0,0,UIntPtr.Zero);Thread.Sleep(40);keybd_event(0x43,0,2,UIntPtr.Zero);Thread.Sleep(30);keybd_event(0x11,0,2,UIntPtr.Zero);}
 async Task Ask(string prompt){if(string.IsNullOrWhiteSpace(selected)){selected=Output.Text;} if(selected.Length==0){Status.Text="请先复制或选择一段文字";return;} var cfg=Config.Load(); if(string.IsNullOrWhiteSpace(cfg.ApiKey)){Status.Text="请先在设置中填写 API Key";return;} Output.Clear();Status.Text="正在生成…";try{var url=cfg.BaseUrl.TrimEnd('/');if(!url.EndsWith("/v1"))url+="/v1";var body=JsonSerializer.Serialize(new{model=cfg.Model,stream=false,messages=new[]{new{role="system",content=prompt},new{role="user",content=selected}}});using var req=new HttpRequestMessage(HttpMethod.Post,url+"/chat/completions");req.Headers.Authorization=new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",cfg.ApiKey);req.Content=new StringContent(body,Encoding.UTF8,"application/json");var res=await Http.SendAsync(req);var raw=await res.Content.ReadAsStringAsync();res.EnsureSuccessStatusCode();var json=JsonDocument.Parse(raw);Output.Text=json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()??"";Status.Text="完成";}catch(Exception e){Status.Text="请求失败："+e.Message;}}
 void Settings_Click(object s,RoutedEventArgs e){var w=new SettingsWindow();w.Owner=this;w.ShowDialog();}
 [DllImport("user32.dll")]static extern bool RegisterHotKey(IntPtr hWnd,int id,uint fs,uint vk);[DllImport("user32.dll")]static extern bool UnregisterHotKey(IntPtr hWnd,int id);[DllImport("user32.dll")]static extern void keybd_event(byte vk,byte scan,uint flags,UIntPtr extra);
}
public sealed class Config { public string BaseUrl {get;set;}="https://api.openai.com/v1"; public string ApiKey {get;set;}=""; public string Model {get;set;}="gpt-4o-mini"; static string PathName=>System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Flick","settings.json"); public static Config Load(){try{return JsonSerializer.Deserialize<Config>(File.ReadAllText(PathName))??new();}catch{return new();}} public void Save(){Directory.CreateDirectory(System.IO.Path.GetDirectoryName(PathName)!);File.WriteAllText(PathName,JsonSerializer.Serialize(this));} }
public sealed class SettingsWindow:Window { TextBox url=new(),key=new(),model=new(); public SettingsWindow(){Title="Flick 设置";Width=440;Height=260;WindowStartupLocation=WindowStartupLocation.CenterOwner;var c=Config.Load();url.Text=c.BaseUrl;key.Text=c.ApiKey;model.Text=c.Model;var p=new StackPanel{Margin=new Thickness(16)};void Field(string label,TextBox box){p.Children.Add(new TextBlock{Text=label,Margin=new Thickness(0,5,0,2)});p.Children.Add(box);}Field("API Base URL",url);Field("API Key",key);Field("模型名称",model);var b=new Button{Content="保存"};b.Click+=(_,_)=>{new Config{BaseUrl=url.Text,ApiKey=key.Text,Model=model.Text}.Save();DialogResult=true;Close();};p.Children.Add(b);Content=p;} }
