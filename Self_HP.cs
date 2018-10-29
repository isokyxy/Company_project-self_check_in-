using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

/// <summary>
/// Self_HP 的摘要说明
/// </summary>
public class Self_HP
{
    public Self_HP()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }


    #region 返回当前酒店的所有房型房价（天价|钟点价）
    public string Return_Fangtype()
    {
        string sql = @" drop table temp_hzb
                        drop table temp_hzb2
                        
                        select * into temp_hzb from(
                        select 房价=(case when(fangjianame = '会员价') then '钟点房价' when (fangjianame = '执行码') then '天价' end ) ,
                        fangjianzhixinjia,fangjianname from jiedai_fangpice_count where fangjianame = '执行码' OR fangjianame = '会员价' 
                        )as a order by 房价
                        
                        declare @sql varchar(8000)
                        set @sql='select * into temp_hzb2 from('
                        select @sql =@sql+ 'select fangjianname'
                        select @sql = @sql + ',isnull (sum(case 房价 when '''+房价+''' then fangjianzhixinjia end),0) as ['+房价+']' 
                        from (select distinct 房价 from temp_hzb) as a
                        select @sql = @sql+' from temp_hzb group by fangjianname) as a'
                        exec(@sql)
                        
                        select  fangjianname,Convert(decimal(18,0),天价) as 天价,Convert(decimal(18,0),钟点房价)as 钟点房价  from temp_hzb2";
        DataSet ds = INI.xb_get_database_record(sql);
        DataTable da_msg = ds.Tables[0];
        string x = Json.DataTableToJson_Ser(da_msg);
        string c = @"{'code':200,'msg':'','List':" + x + @"}";
        c = c.Replace("\'", "\"");
        return c;
    }
    #endregion


    #region 返回当前房型的可用房号
    public string Return_room(string fangjianname)
    {
        string sql = "select room_name,fangname,room_louloop,room_loudong,kefangfenge,id from room where fangname = '" + fangjianname + "' and room_name <> ''";
        DataSet ds = INI.xb_get_database_record(sql);
        DataTable da_msg = ds.Tables[0];
        string x = Json.DataTableToJson_Ser(da_msg);
        string c = @"{'code':200,'msg':'','List':" + x + @"}";
        c = c.Replace("\'", "\"");
        return c;
    }
    #endregion


    #region 客人入住将数据插入_入住表
    public string Insert_usermsg(string krxm, string fanghao, string tiansu, string fangpice, string yajinzonge, string sax, string fangjianname, string room_louloop, string room_loudong, string kefangfenge, string rensu, string id, string fanglei)
    {


        #region 生成账单号
        string zhangdanhao = INI.xb_get_zhangdanhao("ft");
        
        #endregion  

        #region 自己获取参数
        string guapai = "select fangjianguapaijia from jiedai_fangpice_count where fangjianname in (select fangname from room where id ='" + id + "')";
        DataSet ds = INI.xb_get_database_record(guapai);
        DataTable da_guapai = ds.Tables[0];
        string guapaij = da_guapai.Rows[0][0].ToString();
        string zhixin = "select fangjianzhixinjia from jiedai_fangpice_count where fangjianname in (select fangname from room where id ='" + id + "')";
        DataSet ds1 = INI.xb_get_database_record(zhixin);
        DataTable da_guapai1 = ds1.Tables[0];
        string zhixinj = da_guapai1.Rows[0][0].ToString();
        string loucen = "select room_loudong,room_louloop,kefangfenge from room where id = '" + id + "'";
        DataSet dskefang = INI.xb_get_database_record(loucen);
        DataTable da_kefang = dskefang.Tables[0];
        string loucens = da_kefang.Rows[0]["room_louloop"].ToString();
        string loudons = da_kefang.Rows[0]["room_loudong"].ToString();
        string fenge = da_kefang.Rows[0]["kefangfenge"].ToString();

        string x = "";
        if (fanglei.Equals("钟点房"))
        {
            x = "钟点码";
        }
        else if (fanglei.Equals("全天房"))
        {
            x = "执行码";
        }
        //入住时间
        string now = "SELECT convert(char(20),getdate(),120) as now";
        DataSet ds_time = INI.xb_get_database_record(now);
        DataTable da_time = ds_time.Tables[0];
        string timedao = da_time.Rows[0][0].ToString();
        // DateTime datanow = Convert.ToDateTime(timedao);
        //离店时间
        int tian = Convert.ToInt32(tiansu);
        string niri = "SELECT convert(char(20),getdate()+" + tian + ",120)";
        DataSet ds_ni = INI.xb_get_database_record(niri);
        DataTable da_ni = ds_ni.Tables[0];
        string timeni = da_ni.Rows[0][0].ToString();
        //DateTime dataniri = Convert.ToDateTime(timeni);
        //押金总额 房间总额 余额
        int zonge = Convert.ToInt32(yajinzonge);
        int pice_fang = Convert.ToInt32(fangpice);
        int yue = zonge - pice_fang;

        #endregion

        #region 插入入住表
        string sql = @"insert into main_jiedai_ruzhu (zhangdanhao,z_zhangdanhao,fangpice,guapaipice,room_louloop,
                                                      room_loudong,kefangfenge,status,ruzhustatus,
                                                      fanghao,zheqouma,daori,tiansu,yajinyue,niri,
                                                      xieyitype,kerentype,fangjianname)
                                                      Values
                                                      ({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},
                                                       {11},{12},{13},{14},{15},{16},{17})";
        sql = string.Format(sql, INI.psformat(zhangdanhao), INI.psformat(zhangdanhao), INI.psformat(zhixinj), INI.psformat(guapaij),
                            INI.psformat(loucens), INI.psformat(loudons), INI.psformat(kefangfenge), "'在住'", "'散客入住'", INI.psformat(fanghao),
                            INI.psformat(x), INI.psformat(INI.sd(timedao)), INI.psformat(tiansu), yue, INI.psformat(INI.sd(timeni)), "'酒店协议价'", "'自来的散客'",INI.psformat(fangjianname));
        LogUtil.WriteLog("改" + sql);
        DataSet ds_msg = INI.xb_get_database_record(sql);
        DataTable da_msg = ds.Tables[0];
        string c = "";
        if (da_msg.Rows.Count >= 1)
        {
            string sql_fangtaiup = "update room set fangtai ='OC' where id = '" + id + "'";
            INI.xb_exe_sql(sql_fangtaiup);
            c = @"{'code':'200','msg':'成功','List':'" + zhangdanhao + @"'}";
            c = c.Replace("\'", "\"");
        }
        else
        {
            c = Common.handleFail();
        }

        return c;
        #endregion
    }
    #endregion


    #region 录入身份证信息
    public string shenfenzhen(string username, string sex, string minzu, string shenfenzhen, string shenri, string shenfenzhenhao, string begin, string end, string jiguan, string address,string zhangdanhao)
    {

        LogUtil.WriteLog(zhangdanhao);
        #region 获取名字首字母大写
        string skrxm = GetNameDaXie.GetSpellCode(username);
        #endregion

        string sql = "update main_jiedai_ruzhu SET krxm={0},sax = {1},minzu = {2},shenfenzhentype = {3},kerenshenri ={4},shenfenzhenhao = {5},jiguan = {6},changzhudizhi = {7},ekrxm ={8} where zhangdanhao = {9}";

        sql = string.Format(sql, INI.psformat(username), INI.psformat(sex), INI.psformat(minzu), INI.psformat(shenfenzhen), INI.psformat(INI.sdd(shenri)), INI.psformat(shenfenzhenhao),
                            INI.psformat(jiguan),INI.psformat(address),INI.psformat(skrxm),INI.psformat(zhangdanhao));
        LogUtil.WriteLog("fuck" + sql);
        bool isOK = INI.xb_exe_sql(sql);
        string x = "";
        if (isOK)
        {
            x = Common.handleResponse(200, "录入成功");
        }
        else
        {
            x = Common.handleFail();
        }

        return x;
    }
    #endregion


    #region 返回录入后显示信息
    public string retrun_shenfen_msg(string zhangdanhao)
    {
        string sql = "select fangjianname,fanghao,fangpice,yajinyue,rensu,ruzhustatus,convert(char(20),daori,120) as daori,convert(char(20),niri,120) as niri from main_jiedai_ruzhu where zhangdanhao = '" + zhangdanhao+"'";
        DataSet ds_msg = INI.xb_get_database_record(sql);
        DataTable da_msg = ds_msg.Tables[0];
        string x = Json.DataTableToJson_Ser(da_msg);
        string data = @"{'code':200,'msg':'','List':" + x + @"}";
        data = data.Replace("\'", "\"");
        return data;
    }
    #endregion


    #region 账务查询
    public string Select_zhangdan_list(string zhangdanhao)
    {
        string sql = "";
        DataSet ds_msg = INI.xb_get_database_record(sql);
        DataTable da_msg = ds_msg.Tables[0];

        string x = Json.DataTableToJson_Ser(da_msg);
        string data = @"{'code':200,'msg':'','List':" + x + @"}";
        data = data.Replace("\'", "\"");
        return data;
    }
    #endregion


    #region 续房
    #endregion


    #region 退房
    #endregion
}