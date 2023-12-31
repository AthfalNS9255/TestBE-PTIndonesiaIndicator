package com.siadminmovie.service;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.data.domain.Sort;
import org.springframework.stereotype.Service;

import com.siadminmovie.model.Movie;
import com.siadminmovie.repository.MovieRepository;

@Service
public class MovieServiceImpl implements MovieService{
    @Autowired
	private MovieRepository movieRepository;

	@Override
	public List<Movie> getAllMovies() {
		return movieRepository.findAll();
	}

	@Override
	public void saveMovie(Movie movie) {
		this.movieRepository.save(movie);
	}

	@Override
	public Movie getMovieById(int id) {
		Optional<Movie> optional = movieRepository.findById(id);
		Movie movie = null;
		if (optional.isPresent()) {
			movie = optional.get();
		} else {
			throw new RuntimeException(" Movie not found for id :: " + id);
		}
		return movie;
	}

	@Override
	public void deleteMovieById(int id) {
		this.movieRepository.deleteById(id);
	}

	@Override
	public Page<Movie> findMovies(int pageNo, int pageSize, String sortField, 
		String sortDirection, String keyword) {

		Sort sort = sortDirection.equalsIgnoreCase(Sort.Direction.ASC.name()) ? Sort.by(sortField).ascending() :
			Sort.by(sortField).descending();

		// Sort.Direction sort2 = sortDirection.equalsIgnoreCase(Sort.Direction.ASC.name()) ? Sort.Direction.ASC : 
		// 	Sort.Direction.DESC;

		Pageable pageable = PageRequest.of(pageNo - 1, pageSize, sort);
		
		Page<Movie> page = keyword != "" && keyword != null ? 
			this.movieRepository.findByNameContainingIgnoreCase(keyword, pageable) :
			this.movieRepository.findAll(pageable);
		return page;
	}


	
}
