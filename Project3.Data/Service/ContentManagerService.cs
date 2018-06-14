﻿using Microsoft.EntityFrameworkCore;
using Project3.Data.Data;
using Project3.Data.Models;
using Project3.Data.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project3.Data.Service
{
    public class ContentManagerService : IContentManagerService
    {
        private readonly Project3DB _project3DB;

        public ContentManagerService(Project3DB _project3DB)
        {
            this._project3DB = _project3DB;
        }

        public Task<Contents> AddAsync(Contents _contents)
        {
            return _project3DB.AddAsync(_contents);
        }



        public async Task<Contents> UpdateAsync(Contents _contents)
        {

            if (_contents.cid > 0)
            {
                return await _project3DB.UpdateAsync(_contents);
            }
            else
            {
                return await _project3DB.AddAsync(_contents);
            }
        }

        public async Task<PaginatedList<Contents>> GetContentsAsync(int status, string searchstring, int metaid, int? pageindex, int pagesize, int type)
        {
            var data = from s in _project3DB.Contents.Where(m => m.type == type) select s;
            if (status >= 0)
            {
                data = data.Where(m => m.status == status);
            }
            if (!String.IsNullOrEmpty(searchstring))
            {
                data = data.Where(m => m.title.Contains(searchstring) ||
                m.content.Contains(searchstring) ||
                m.excerpt.Contains(searchstring));
            }
            if (metaid > 0)
            {
                var cg = await _project3DB.Metas
                 .AsNoTracking()
                 .SingleOrDefaultAsync(m => m.mid == metaid);
                if (cg != null)
                {
                    data = (from s in data join r in _project3DB.Relationships on s.cid equals r.cid where r.mid == metaid select s);
                }
            }
            data = data.OrderByDescending(m => m.cid);

            return await PaginatedList<Contents>.CreateAsync(data.AsNoTracking(), pageindex ?? 1, pagesize);

        }

        public async Task<int> DeleteByCidAsync(int[] cid)
        {
            foreach (int id in cid)
            {
                var content = await _project3DB.Contents.Where(m => m.cid == id).SingleOrDefaultAsync();
                if (content != null)
                {
                    _project3DB.Contents.Remove(content);
                }
            }
            return await _project3DB.SaveChangesAsync();
        }

        public async Task<Contents> GetByCidAsync(int cid)
        {
            return await _project3DB.Contents.Where(m => m.cid == cid).SingleOrDefaultAsync();

        }

        public async Task<IList<Metas>> GetMetasAsync(int cid)
        {
            var metas = (from s in _project3DB.Metas join r in _project3DB.Relationships on s.mid equals r.mid where r.cid == cid select s);

            return await metas.ToListAsync();
        }

        public async Task UpdateMetasByCidAsync(int cid, string[] name, int type)
        {
            var metas = new List<Metas>();
            foreach (string n in name)
            {
                if (!string.IsNullOrEmpty(n))
                {
                    var meta = await _project3DB.Metas.Where(m => m.name == n && m.type == type).SingleOrDefaultAsync();
                    if (meta == null)
                    {
                        meta = new Metas();
                        meta.name = n;
                        meta.type = type;
                        meta.count = 1;
                        meta = await _project3DB.AddAsync(meta);
                    }

                    metas.Add(meta);
                    var relationship = await _project3DB.Relationships.Where(m => m.cid == cid && m.mid == meta.mid).SingleOrDefaultAsync();
                    if (relationship == null)
                    {
                        relationship = new Relationships();
                        relationship.cid = cid;
                        relationship.mid = meta.mid;
                        await _project3DB.AddAsync(relationship);
                        //更新文章计数
                        meta.count++;
                        meta = await _project3DB.UpdateAsync(meta);
                    }
                }
            }


            //删除掉旧的分类/标签
            var r = (from s in _project3DB.Relationships where s.cid == cid join m in _project3DB.Metas on s.mid equals m.mid where m.type == type select s);
            foreach (Relationships rs in r)
            {
                if (metas.Where(m => m.mid == rs.mid).Count() == 0)
                {
                    //更新文章计数
                    var meta = await _project3DB.Metas.Where(m => m.mid == rs.mid).SingleOrDefaultAsync();
                    if (meta != null)
                    {
                        meta.count--;
                        _project3DB.Metas.Update(meta);
                    }
                    _project3DB.Relationships.Remove(rs);

                }

            }
            await _project3DB.SaveChangesAsync();
        }

        public async Task<Dictionary<string, IList<Contents>>> GetArchivesAsync()
        {
            var data = await (from s in _project3DB.Contents select s).Select(s => new Contents { cid = s.cid, title = s.title, createtime = s.createtime }).OrderByDescending(m => m.createtime).ToListAsync();

            var resultdata = new Dictionary<string, IList<Contents>>();
            foreach (var content in data)
            {
                string year = content.createtime.ToString("yyyy");
                if (resultdata.ContainsKey(year))
                {
                    resultdata[year].Add(content);
                }
                else
                {
                    resultdata.Add(year, new List<Contents>() { content });
                }
            }

            return resultdata;
        }

        public async Task<Contents> GetPageByCidAsync(int cid)
        {
            var data = await _project3DB.Contents.Where(m => m.cid == cid && m.type == 1).Select(m => new Contents { title = m.title, content = m.content }).SingleOrDefaultAsync();
            return data;
        }

        public async Task<IList<Contents>> GetPageListAsync()
        {
            var data = await (from s in _project3DB.Contents.Where(m => m.type == 1).OrderByDescending(m => m.index).ThenByDescending(m => m.createtime) select s).ToListAsync();
            return data;
        }
    }
}